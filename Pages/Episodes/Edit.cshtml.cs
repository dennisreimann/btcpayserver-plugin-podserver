using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Client;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTCPayServer.Plugins.PodServer.Pages.Episodes;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class EditModel : BasePageModel
{
    private readonly IFileService _fileService;
    public IEnumerable<SelectListItem> SeasonItems { get; set; }
    public Episode Episode { get; set; }
    public IFormFile ImageFile { get; set; }
    public IFormFile EnclosureFile { get; set; }

    public EditModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository, IFileService fileService) : base(userManager, podcastRepository)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGet(string podcastId, string episodeId)
    {
        Episode = await GetEpisode(podcastId, episodeId);
        if (Episode == null) return NotFound();

        SeasonItems = await GetSeasonItems(Episode.PodcastId);

        if (Episode.Number == null)
        {
            var episodes = await PodcastRepository.GetEpisodes(new EpisodesQuery
            {
                PodcastId = podcastId
            });
            var highestNumber = episodes.MaxBy(e => e.Number)?.Number ?? 0;
            Episode.Number = highestNumber + 1;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string episodeId)
    {
        Episode = await GetEpisode(podcastId, episodeId);
        if (Episode == null) return NotFound();
        
        SeasonItems = await GetSeasonItems(Episode.PodcastId);
        
        if (!ModelState.IsValid) return Page();
        
        if (ImageFile != null)
        {
            // delete existing image
            if (!string.IsNullOrEmpty(Episode.ImageFileId))
            {
                await _fileService.RemoveFile(Episode.ImageFileId, UserId);
            }
            // add new image
            try
            {
                var storedFile = await _fileService.AddFile(ImageFile, UserId);
                Episode.ImageFileId = storedFile.Id;
            }
            catch (Exception e)
            {
                TempData[WellKnownTempData.ErrorMessage] = $"Could not save image: {e.Message}";
            }
        }
        
        if (EnclosureFile != null)
        {
            // delete existing enclosure
            // TODO: Allow for multiple enclosures
            if (Episode.Enclosures.Any())
            {
                foreach (var enclosure in Episode.Enclosures)
                {
                    await _fileService.RemoveFile(enclosure.FileId, UserId);
                    Episode.Enclosures.Remove(enclosure);
                }
            }
            // add new enclosure
            try
            {
                var storedFile = await _fileService.AddFile(EnclosureFile, UserId);
                var enclosure = new Enclosure
                {
                    EpisodeId = Episode.EpisodeId,
                    FileId = storedFile.Id,
                    Type = EnclosureFile.ContentType,
                    Length = EnclosureFile.Length
                };
                Episode.Enclosures.Add(enclosure);
            }
            catch (Exception e)
            {
                TempData[WellKnownTempData.ErrorMessage] = $"Could not save media file: {e.Message}";
            }
        }

        if (await TryUpdateModelAsync(
            Episode, 
            "episode",
            e => e.Title,
            e => e.Description,
            e => e.Number,
            e => e.SeasonId,
            e => e.ImageFileId,
            e => e.PublishedAt,
            e => e.Enclosures))
        {
            Episode.LastUpdatedAt = DateTimeOffset.UtcNow; 
            await PodcastRepository.AddOrUpdateEpisode(Episode);
            if (TempData[WellKnownTempData.ErrorMessage] is null)
            {
                TempData[WellKnownTempData.SuccessMessage] = "Episode successfully updated.";
            }
        
            return RedirectToPage("./Episode", new { podcastId = Episode.PodcastId, episodeId = Episode.EpisodeId });
        }
        
        return Page();
    }

    private async Task<Episode> GetEpisode(string podcastId, string episodeId)
    {
        return await PodcastRepository.GetEpisode(new EpisodesQuery
        {
            PodcastId = podcastId,
            EpisodeId = episodeId,
            IncludeEnclosures = true
        });
    }

    private async Task<IEnumerable<SelectListItem>> GetSeasonItems(string podcastId)
    {
        var seasons = await PodcastRepository.GetSeasons(new SeasonsQuery { PodcastId = podcastId });
        return seasons.Select(s => new SelectListItem { Value = s.SeasonId, Text = s.Number + (string.IsNullOrEmpty(s.Name) ? "" : $" - {s.Name}") });
    }
}
