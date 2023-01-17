using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Public;

[AllowAnonymous]
public class PublicPodcastModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode LatestEpisode { get; set; }
    public IEnumerable<Episode> MoreEpisodes { get; set; }

    public PublicPodcastModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGetAsync(string podcastSlug)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            Slug = podcastSlug
        });
        if (Podcast == null)
            return NotFound();

        var episodes = (await PodcastRepository.GetEpisodes(new EpisodesQuery
        {
            PodcastId = Podcast.PodcastId,
            OnlyPublished = true
        })).ToList();

        if (episodes.Any())
        {
            LatestEpisode = episodes.First();
            MoreEpisodes = episodes.Skip(1);
        }

        return Page();
    }
}
