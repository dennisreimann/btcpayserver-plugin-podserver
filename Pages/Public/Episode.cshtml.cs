using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Public;

[AllowAnonymous]
public class PublicEpisodeModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }

    public PublicEpisodeModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGetAsync(string podcastSlug, string episodeSlug)
    {
        Episode = await PodcastRepository.GetEpisode(new EpisodesQuery
        {
            Slug = episodeSlug,
            IncludePodcast = true,
            IncludeContributions = true,
            IncludeEnclosures = true,
            IncludeSeason = true
        });

        Podcast = Episode.Podcast;

        if (!Podcast.Slug.Equals(podcastSlug))
            return NotFound();
        if (!Episode.IsPublished)
            return NotFound();

        return Page();
    }
}
