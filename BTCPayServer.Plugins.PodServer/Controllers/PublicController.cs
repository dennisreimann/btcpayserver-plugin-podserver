using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Filters;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Plugins.PodServer.ViewModels;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Controllers;

[AllowAnonymous]
[AutoValidateAntiforgeryToken]
public class PublicController : PublicBaseController
{
    public PublicController(
        AppService appService,
        PodcastRepository podcastRepository) : base(appService, podcastRepository) { }

    [HttpGet("/")]
    [HttpGet("/plugins/podserver/podcast/{podcastSlug}")]
    [XFrameOptions(XFrameOptionsAttribute.XFrameOptions.Unset)]
    [DomainMappingConstraint(PodServerApp.AppType)]
    public async Task<IActionResult> ViewPodcast(string podcastSlug)
    {
        var podcast = await GetPodcast(new PodcastsQuery { Slug = podcastSlug });
        if (podcast == null)
            return NotFound();

        var vm = new PublicPodcastViewModel { Podcast = podcast };

        var episodes = (await PodcastRepository.GetEpisodes(new EpisodesQuery
        {
            PodcastId = podcast.PodcastId,
            OnlyPublished = true
        })).ToList();

        if (episodes.Any())
        {
            vm.LatestEpisode = episodes.First();
            vm.MoreEpisodes = episodes.Skip(1);
        }

        return View("Podcast", vm);
    }

    [HttpGet("/episode/{episodeSlug}")]
    [HttpGet("/plugins/podserver/podcast/{podcastSlug}/episode/{episodeSlug}")]
    [XFrameOptions(XFrameOptionsAttribute.XFrameOptions.Unset)]
    public async Task<IActionResult> ViewEpisode(string podcastSlug, string episodeSlug)
    {
        var episode = await PodcastRepository.GetEpisode(new EpisodesQuery
        {
            Slug = episodeSlug,
            IncludePodcast = true,
            IncludeContributions = true,
            IncludeEnclosures = true,
            IncludeSeason = true
        });

        if (!episode.IsPublished)
            return NotFound();

        var podcast = episode.Podcast;
        if (!string.IsNullOrEmpty(podcastSlug) && !podcast.Slug.Equals(podcastSlug))
            return NotFound();

        var vm = new PublicEpisodeViewModel
        {
            Podcast = podcast,
            Episode = episode
        };

        return View("Episode", vm);
    }
}
