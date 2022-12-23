using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanView)]
public class PodcastModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public IEnumerable<Episode> Episodes { get; set; }

    public PodcastModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGetAsync(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId,
            IncludeEpisodes = true
        });
        if (Podcast == null)
            return NotFound();

        Episodes = Podcast.Episodes.OrderByDescending(t => t.PublishedAt);

        return Page();
    }
}
