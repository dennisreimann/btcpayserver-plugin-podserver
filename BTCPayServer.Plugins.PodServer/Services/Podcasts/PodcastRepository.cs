using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Plugins.PodServer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BTCPayServer.Plugins.PodServer.Services.Podcasts;

public class PodcastRepository
{
    private readonly IFileService _fileService;
    private readonly PodServerPluginDbContextFactory _dbContextFactory;

    public PodcastRepository(
        IFileService fileService,
        PodServerPluginDbContextFactory dbContextFactory)
    {
        _fileService = fileService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Podcast>> GetPodcasts(PodcastsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        var podcasts = await FilterPodcasts(dbContext.Podcasts.AsQueryable(), query).ToListAsync();

        return podcasts.Select(podcast =>
        {
            var editor = podcast.Editors.FirstOrDefault(editor => query.UserId.Contains(editor.UserId));
            if (query.UserId != null && query.UserId.Contains(podcast.OwnerId))
            {
                podcast.Role = EditorRole.Admin;
            }
            else if (editor != null)
            {
                podcast.Role = editor.Role;
            }
            return podcast;
        });
    }

    public async Task<Podcast> GetPodcast(PodcastsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        var podcast = await FilterPodcasts(dbContext.Podcasts.AsQueryable(), query).FirstOrDefaultAsync();
        if (podcast == null)
            return null;

        if (query.UserId != null)
        {
            var editor = podcast.Editors.FirstOrDefault(editor => query.UserId.Contains(editor.UserId));
            if (query.UserId.Contains(podcast.OwnerId))
            {
                podcast.Role = EditorRole.Admin;
            }
            else if (editor != null)
            {
                podcast.Role = editor.Role;
            }
        }

        return podcast;
    }

    private IQueryable<Podcast> FilterPodcasts(IQueryable<Podcast> queryable, PodcastsQuery query)
    {
        if (!string.IsNullOrEmpty(query.UserId))
        {
            queryable = queryable.Include(podcast => podcast.Editors)
                .Where(p =>
                    // Owner
                    query.UserId.Contains(p.OwnerId) ||
                    // Editor
                    p.Editors.SingleOrDefault(e => e.UserId == query.UserId) != null);
        }

        if (!string.IsNullOrEmpty(query.PodcastId))
        {
            queryable = queryable.Where(p => p.PodcastId == query.PodcastId);
        }

        if (!string.IsNullOrEmpty(query.Slug))
        {
            queryable = queryable.Where(p => p.Slug == query.Slug);
        }

        if (query.IncludeEpisodes)
        {
            queryable = queryable.Include(p => p.Episodes);
        }

        if (query.IncludeSeasons)
        {
            queryable = queryable.Include(p => p.Seasons);
        }

        if (query.IncludePeople)
        {
            queryable = queryable.Include(p => p.People);
        }

        if (query.IncludeImports)
        {
            queryable = queryable.Include(p => p.Imports);
        }

        if (query.IncludeEditors)
        {
            queryable = queryable.Include(p => p.Editors);
        }

        if (query.IncludeContributions)
        {
            queryable = queryable.Include(p => p.Contributions
                .Where(c => c.EpisodeId == null));
        }

        return queryable;
    }

    public async Task<Podcast> AddOrUpdatePodcast(Podcast podcast)
    {
        await using var dbContext = _dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(podcast.PodcastId))
        {
            entry = await dbContext.Podcasts.AddAsync(podcast);
        }
        else
        {
            entry = dbContext.Update(podcast);
        }
        await dbContext.SaveChangesAsync();

        return (Podcast)entry.Entity;
    }

    public async Task RemovePodcast(Podcast podcast)
    {
        if (!string.IsNullOrEmpty(podcast.ImageFileId))
        {
            await _fileService.RemoveFile(podcast.ImageFileId, null);
        }

        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.Podcasts.Remove(podcast);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Episode>> GetEpisodes(EpisodesQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterEpisodes(dbContext.Episodes.AsQueryable(), query).ToListAsync();
    }

    public async Task<Episode> GetEpisode(EpisodesQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterEpisodes(dbContext.Episodes.AsQueryable(), query).FirstOrDefaultAsync();
    }

    private IQueryable<Episode> FilterEpisodes(IQueryable<Episode> queryable, EpisodesQuery query)
    {
        if (query.IncludePeople)
        {
            query.IncludePodcast = true;
        }

        if (!string.IsNullOrEmpty(query.PodcastId))
        {
            query.IncludePodcast = true;

            queryable = queryable.Where(e => e.PodcastId == query.PodcastId);
        }

        if (query.OnlyPublished)
        {
            query.IncludeEnclosures = true;
        }

        if (query.IncludePodcast)
        {
            queryable = query.IncludePeople
                ? queryable.Include(e => e.Podcast).ThenInclude(p => p.People)
                : queryable.Include(e => e.Podcast);
        }

        if (!string.IsNullOrEmpty(query.EpisodeId))
        {
            queryable = queryable.Where(e => e.EpisodeId == query.EpisodeId);
        }

        if (!string.IsNullOrEmpty(query.Slug))
        {
            queryable = queryable.Where(e => e.Slug == query.Slug);
        }

        if (!string.IsNullOrEmpty(query.SeasonId))
        {
            query.IncludeSeason = true;

            queryable = queryable.Where(e => e.SeasonId == query.SeasonId);
        }

        if (!string.IsNullOrEmpty(query.ImportGuid))
        {
            queryable = queryable.Where(e => e.ImportGuid == query.ImportGuid);
        }

        if (query.IncludeSeason)
        {
            queryable = queryable.Include(e => e.Season);
        }

        if (query.IncludeEnclosures)
        {
            queryable = queryable.Include(e => e.Enclosures);
        }

        if (query.IncludeContributions)
        {
            queryable = queryable.Include(e => e.Contributions);
        }

        if (query.OnlyPublished)
        {
            queryable = queryable
                    .Where(e => e.PublishedAt != null && e.PublishedAt <= DateTime.UtcNow &&
                                e.Enclosures.FirstOrDefault(enc => !enc.IsAlternate) != null);
        }

        queryable = queryable.OrderByDescending(t => t.PublishedAt);

        return queryable;
    }

    public async Task<Episode> AddOrUpdateEpisode(Episode episode)
    {
        await using var dbContext = _dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(episode.EpisodeId))
        {
            entry = await dbContext.Episodes.AddAsync(episode);
        }
        else
        {
            entry = dbContext.Update(episode);
        }
        await dbContext.SaveChangesAsync();

        return (Episode)entry.Entity;
    }

    public async Task RemoveEpisode(Episode episode)
    {
        if (!string.IsNullOrEmpty(episode.ImageFileId))
        {
            await _fileService.RemoveFile(episode.ImageFileId, null);
        }

        if (episode.Enclosures.Any())
        {
            foreach (var enclosure in episode.Enclosures)
            {
                await _fileService.RemoveFile(enclosure.FileId, null);
            }
        }

        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.Episodes.Remove(episode);
        await dbContext.SaveChangesAsync();
    }

    private async Task<IEnumerable<Person>> GetPeople(PeopleQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterPeople(dbContext.People.AsQueryable(), query).ToListAsync();
    }

    public async Task<Person> GetPerson(PeopleQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterPeople(dbContext.People.AsQueryable(), query).FirstOrDefaultAsync();
    }

    private IQueryable<Person> FilterPeople(IQueryable<Person> queryable, PeopleQuery query)
    {
        if (query.PodcastId != null)
        {
            queryable = queryable.Where(p => p.PodcastId == query.PodcastId);
        }

        if (query.PersonId != null)
        {
            queryable = queryable.Where(p => p.PersonId == query.PersonId);
        }

        if (!string.IsNullOrEmpty(query.Name))
        {
            queryable = queryable.Where(p => string.Equals(p.Name, query.Name));
        }

        if (query.IncludeContributions)
        {
            queryable = queryable
                .Include(p => p.Contributions.Where(c => c.EpisodeId == null));
        }

        return queryable;
    }

    public async Task<Person> AddOrUpdatePerson(Person person)
    {
        await using var dbContext = _dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(person.PersonId))
        {
            entry = await dbContext.People.AddAsync(person);
        }
        else
        {
            entry = dbContext.Update(person);
        }
        await dbContext.SaveChangesAsync();

        return (Person)entry.Entity;
    }

    public async Task RemovePerson(Person person)
    {
        if (!string.IsNullOrEmpty(person.ImageFileId))
        {
            await _fileService.RemoveFile(person.ImageFileId, null);
        }

        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.People.Remove(person);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Season>> GetSeasons(SeasonsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterSeasons(dbContext.Seasons.AsQueryable(), query).ToListAsync();
    }

    public async Task<Season> GetSeason(SeasonsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterSeasons(dbContext.Seasons.AsQueryable(), query).FirstOrDefaultAsync();
    }

    private IQueryable<Season> FilterSeasons(IQueryable<Season> queryable, SeasonsQuery query)
    {
        if (query.PodcastId != null)
        {
            queryable = queryable.Where(s => s.PodcastId == query.PodcastId);
        }

        if (query.SeasonId != null)
        {
            queryable = queryable.Where(s => s.SeasonId == query.SeasonId);
        }

        if (query.Number != 0)
        {
            queryable = queryable.Where(s => s.Number == query.Number);
        }

        return queryable;
    }

    public async Task<Season> AddOrUpdateSeason(Season season)
    {
        await using var dbContext = _dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(season.SeasonId))
        {
            entry = await dbContext.Seasons.AddAsync(season);
        }
        else
        {
            entry = dbContext.Update(season);
        }
        await dbContext.SaveChangesAsync();

        return (Season)entry.Entity;
    }

    public async Task RemoveSeason(Season season)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.Seasons.Remove(season);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Contribution>> GetContributions(ContributionsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterContributions(dbContext.Contributions.AsQueryable(), query).ToListAsync();
    }

    public async Task<Contribution> GetContribution(ContributionsQuery query)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        return await FilterContributions(dbContext.Contributions.AsQueryable(), query).FirstOrDefaultAsync();
    }

    private IQueryable<Contribution> FilterContributions(IQueryable<Contribution> queryable, ContributionsQuery query)
    {
        if (!string.IsNullOrEmpty(query.PodcastId))
        {
            queryable = queryable.Where(c => c.PodcastId == query.PodcastId);
        }

        if (query.PodcastOnly)
        {
            queryable = queryable.Where(c => c.EpisodeId == null);
        }

        if (!string.IsNullOrEmpty(query.EpisodeId))
        {
            queryable = queryable.Where(c => c.EpisodeId == query.EpisodeId);
        }

        if (!string.IsNullOrEmpty(query.PersonId))
        {
            queryable = queryable.Where(c => c.PersonId == query.PersonId);
        }

        if (query.IncludePerson)
        {
            queryable = queryable.Include(c => c.Person);
        }

        return queryable;
    }

    public async Task<Contribution> AddOrUpdateContribution(Contribution contribution)
    {
        await using var dbContext = _dbContextFactory.CreateContext();

        EntityEntry entry;
        if (string.IsNullOrEmpty(contribution.ContributionId))
        {
            entry = await dbContext.Contributions.AddAsync(contribution);
        }
        else
        {
            entry = dbContext.Update(contribution);
        }
        await dbContext.SaveChangesAsync();

        return (Contribution)entry.Entity;
    }

    public async Task RemoveContribution(Contribution contribution)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.Contributions.Remove(contribution);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Editor> AddOrUpdateEditor(string podcastId, string userId, EditorRole role)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        var editor = await dbContext.Editors.FirstOrDefaultAsync(e => e.PodcastId == podcastId && e.UserId == userId);

        if (editor == null)
        {
            editor = new Editor
            {
                UserId = userId,
                PodcastId = podcastId,
                Role = role
            };
            await dbContext.Editors.AddAsync(editor);
        }
        else if (editor.Role != role)
        {
            editor.Role = role;
            dbContext.Update(editor);
        }
        await dbContext.SaveChangesAsync();

        return editor;
    }

    public async Task RemoveEditor(string podcastId, string userId)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        var editor = await dbContext.Editors.FirstAsync(a => a.PodcastId == podcastId && a.UserId == userId);
        await RemoveEditor(editor);
    }

    public async Task RemoveEditor(Editor editor)
    {
        await using var dbContext = _dbContextFactory.CreateContext();
        dbContext.Editors.Remove(editor);
        await dbContext.SaveChangesAsync();
    }
}
