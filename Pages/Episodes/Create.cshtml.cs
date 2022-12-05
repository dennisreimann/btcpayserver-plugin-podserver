using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Extensions;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Episodes;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class CreateModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }

    public CreateModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) {}

    public async Task<IActionResult> OnGet(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery { UserId = UserId, PodcastId = podcastId });
        if (Podcast == null) return NotFound();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery { UserId = UserId, PodcastId = podcastId });
        if (Podcast == null) return NotFound();
        
        if (!ModelState.IsValid) return Page();

        Episode = new Episode { PodcastId = Podcast.PodcastId };

        if (await TryUpdateModelAsync(
            Episode, 
            "episode",
            e => e.PodcastId,
            e => e.Title,
            e => e.Description))
        {
            Episode.Slug = Episode.Title.Slugify();
            
            Episode.LastUpdatedAt = DateTimeOffset.UtcNow;
            await PodcastRepository.AddOrUpdateEpisode(Episode);
        
            TempData[WellKnownTempData.SuccessMessage] = "Episode successfully created.";
            return RedirectToPage("./Edit", new { podcastId = Podcast.PodcastId, episodeId = Episode.EpisodeId });
        }
        
        return Page();
    }
}
