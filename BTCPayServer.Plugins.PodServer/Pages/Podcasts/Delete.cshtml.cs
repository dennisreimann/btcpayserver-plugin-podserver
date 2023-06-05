using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManagePodcast)]
public class DeleteModel : BasePageModel
{
    private readonly AppService _appService;

    public Podcast Podcast { get; set; }

    public DeleteModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository,
        AppService appService) : base(userManager, podcastRepository) {
        _appService = appService;
    }

    public async Task<IActionResult> OnGet(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId
        });
        if (Podcast == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId,
            IncludeEpisodes = true,
            IncludePeople = true,
            IncludeSeasons = true,
            IncludeContributions = true
        });
        if (Podcast == null)
            return NotFound();

        // Delete associated app
        var app = (await _appService.GetApps(PodServerApp.AppType)).FirstOrDefault(appData =>
            appData.GetSettings<PodServerSettings>().PodcastId == Podcast.PodcastId);
        if (app != null)
        {
            await _appService.DeleteApp(app);
        }

        // Delete the podcast
        await PodcastRepository.RemovePodcast(Podcast);
        TempData[WellKnownTempData.SuccessMessage] = "Podcast successfully deleted.";

        return RedirectToPage("./Index");
    }
}
