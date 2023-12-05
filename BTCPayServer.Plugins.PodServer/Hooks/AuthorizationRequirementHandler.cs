using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace BTCPayServer.Plugins.PodServer.Hooks;

public class AuthorizationRequirementHandler(
    UserManager<ApplicationUser> userManager,
    PodcastRepository podcastRepository)
    : IPluginHookFilter
{
    public string Hook { get; } = "handle-authorization-requirement";

    public async Task<object> Execute(object args)
    {
        var obj = (AuthorizationFilterHandle)args;
        var httpContext = obj.HttpContext;
        var userId = userManager.GetUserId(obj.Context.User);
        Podcast podcast = null;

        var routeData = httpContext.GetRouteData();
        if (routeData.Values.TryGetValue("podcastId", out var vPodcastId) && vPodcastId is string podcastId)
        {
            podcast = await podcastRepository.GetPodcast(new PodcastsQuery
            {
                UserId = userId,
                PodcastId = podcastId
            });
        }

        switch (obj.Requirement.Policy)
        {
            case PodServerPolicies.CanManagePodcast:
                if (podcast is { Role: EditorRole.Admin })
                    obj.MarkSuccessful();
                break;
            case PodServerPolicies.CanManageEpisodes:
                if (podcast is { Role: EditorRole.Admin } or { Role: EditorRole.Editor })
                    obj.MarkSuccessful();
                break;
            case PodServerPolicies.CanView:
                if (podcast != null)
                    obj.MarkSuccessful();
                break;
        }

        if (obj.Success)
        {
            httpContext.Items["BTCPAY.PODSERVER.PODCAST"] = podcast;
        }

        return obj;
    }
}


