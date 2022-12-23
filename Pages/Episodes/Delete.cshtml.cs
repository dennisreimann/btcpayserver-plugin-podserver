using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Client;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Episodes;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class DeleteModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }

    public DeleteModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId, string episodeId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId
        });
        if (Podcast == null)
            return NotFound();

        Episode = await PodcastRepository.GetEpisode(new EpisodesQuery
        {
            PodcastId = podcastId,
            EpisodeId = episodeId
        });
        if (Episode == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string episodeId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId
        });
        if (Podcast == null)
            return NotFound();

        Episode = await PodcastRepository.GetEpisode(new EpisodesQuery
        {
            PodcastId = podcastId,
            EpisodeId = episodeId,
            IncludeContributions = true,
            IncludeEnclosures = true
        });
        if (Episode == null)
            return NotFound();

        await PodcastRepository.RemoveEpisode(Episode);
        TempData[WellKnownTempData.SuccessMessage] = "Episode successfully deleted.";

        return RedirectToPage("/Podcasts/Podcast", new { podcastId = Podcast.PodcastId });
    }
}
