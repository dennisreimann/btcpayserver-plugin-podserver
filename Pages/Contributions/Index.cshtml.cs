using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Contributions;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanView)]
public class IndexModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }
    public IEnumerable<Contribution> Contributions { get; set; }
    public bool HasPeople { get; set; }

    public IndexModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) {}

    public async Task<IActionResult> OnGet(string podcastId, string episodeId)
    {
        if (string.IsNullOrEmpty(episodeId))
        {
            Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery {
                UserId = UserId,
                PodcastId = podcastId,
                IncludePeople = true,
                IncludeContributions = true
            });
            Contributions = Podcast.Contributions.OrderByDescending(c => c.Person.Name);
        }
        else
        {
            Episode = await PodcastRepository.GetEpisode(new EpisodesQuery {
                PodcastId = podcastId,
                EpisodeId = episodeId,
                IncludePeople = true,
                IncludePodcast = true,
                IncludeContributions = true
            });
            Podcast = Episode.Podcast;
            Contributions = Episode.Contributions.OrderByDescending(c => c.Person.Name);
        }

        HasPeople = Podcast.People.Any();
        
        return Page();
    }
}
