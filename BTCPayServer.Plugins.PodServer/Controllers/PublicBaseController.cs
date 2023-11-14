using System.Threading.Tasks;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Controllers;

[AllowAnonymous]
public class PublicBaseController : Controller
{
    protected readonly AppService AppService;
    protected readonly PodcastRepository PodcastRepository;

    public PublicBaseController(
        AppService appService,
        PodcastRepository podcastRepository)
    {
        AppService = appService;
        PodcastRepository = podcastRepository;
    }

    protected async Task<Podcast> GetPodcast(PodcastsQuery query)
    {
        var appId = (string)ControllerContext.RouteData.Values["appId"];
        if (!string.IsNullOrEmpty(query.PodcastSlug))
            return await PodcastRepository.GetPodcast(query);
        if (!string.IsNullOrEmpty(appId))
            return await PodcastForApp(appId, query);
        return null;
    }

    private async Task<Podcast> PodcastForApp(string appId, PodcastsQuery query)
    {
        var app = await AppService.GetApp(appId, PodServerApp.AppType);
        var config = app?.GetSettings<PodServerSettings>();
        if (string.IsNullOrEmpty(config?.PodcastId)) return null;
        query.PodcastSlug = null; // ensure slug isn't set
        query.PodcastId = config.PodcastId;
        return await PodcastRepository.GetPodcast(query);
    }
}
