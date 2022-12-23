using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Seasons;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class DeleteModel : BasePageModel
{
    public Season Season { get; set; }

    public DeleteModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId, string seasonId)
    {
        Season = await PodcastRepository.GetSeason(new SeasonsQuery
        {
            PodcastId = podcastId,
            SeasonId = seasonId
        });
        if (Season == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string seasonId)
    {
        Season = await PodcastRepository.GetSeason(new SeasonsQuery
        {
            PodcastId = podcastId,
            SeasonId = seasonId
        });
        if (Season == null)
            return NotFound();

        await PodcastRepository.RemoveSeason(Season);
        TempData[WellKnownTempData.SuccessMessage] = "Season successfully deleted.";

        return RedirectToPage("./Index", new { podcastId = Season.PodcastId });
    }
}
