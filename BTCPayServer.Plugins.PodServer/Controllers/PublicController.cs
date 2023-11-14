using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Filters;
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
    public PublicController(AppService appService, PodcastRepository podcastRepository) : base(appService, podcastRepository) { }

    [HttpGet("/plugins/podserver/podcast/{podcastSlug}")]
    public async Task<IActionResult> Podcast(string podcastSlug)
    {
        return await PodcastResult(new PodcastsQuery { PodcastSlug = podcastSlug });
    }

    [HttpGet("/")]
    [HttpGet("/apps/{appId}/podcast")]
    [DomainMappingConstraint(PodServerApp.AppType)]
    public async Task<IActionResult> PodcastApp(string appId)
    {
        var app = await AppService.GetApp(appId, PodServerApp.AppType);
        if (app == null) return NotFound();

        var settings = app.GetSettings<PodServerSettings>();
        return await PodcastResult(new PodcastsQuery { PodcastId = settings.PodcastId });
    }

    private async Task<IActionResult> PodcastResult(PodcastsQuery podcastsQuery)
    {
        var podcast = await GetPodcast(podcastsQuery);
        if (podcast == null) return NotFound();

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

    [HttpGet("/plugins/podserver/podcast/{podcastSlug}/episode/{episodeSlug}")]
    public async Task<IActionResult> Episode(string podcastSlug, string episodeSlug)
    {
        return await EpisodeResult(new EpisodesQuery { PodcastSlug = podcastSlug, EpisodeSlug = episodeSlug });
    }

    [HttpGet("/episode/{episodeSlug}")]
    [HttpGet("/apps/{appId}/podcast/episode/{episodeSlug}")]
    [DomainMappingConstraint(PodServerApp.AppType)]
    public async Task<IActionResult> EpisodeApp(string appId, string episodeSlug)
    {
        var app = await AppService.GetApp(appId, PodServerApp.AppType);
        if (app == null) return NotFound();

        var settings = app.GetSettings<PodServerSettings>();
        return await EpisodeResult(new EpisodesQuery { PodcastId = settings.PodcastId, EpisodeSlug = episodeSlug });
    }

    private async Task<IActionResult> EpisodeResult(EpisodesQuery episodesQuery)
    {
        episodesQuery.IncludePodcast = true;
        episodesQuery.IncludeContributions = true;
        episodesQuery.IncludeEnclosures = true;
        episodesQuery.IncludeSeason = true;
        episodesQuery.OnlyPublished = true;

        var episode = await PodcastRepository.GetEpisode(episodesQuery);
        if (episode is not { IsPublished: true })
            return NotFound();

        var vm = new PublicEpisodeViewModel
        {
            Podcast = episode.Podcast,
            Episode = episode
        };

        return View("Episode", vm);
    }
}
