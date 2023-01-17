using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Seasons;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class EditModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Season Season { get; set; }

    public EditModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository, IFileService fileService) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId, string seasonId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId,
            IncludeSeasons = true
        });
        if (Podcast == null)
            return NotFound();

        if (seasonId == null)
        {
            Season = new Season
            {
                PodcastId = podcastId,
                Number = Podcast.Seasons
                    .OrderByDescending(s => s.Number)
                    .Select(s => s.Number)
                    .FirstOrDefault(0) + 1
            };
        }
        else
        {
            Season = await PodcastRepository.GetSeason(new SeasonsQuery
            {
                PodcastId = podcastId,
                SeasonId = seasonId,
            });
            if (Season == null)
                return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string seasonId)
    {
        var isNew = seasonId == null;
        if (isNew)
        {
            Season = new Season { PodcastId = podcastId };
        }
        else
        {
            Season = await PodcastRepository.GetSeason(new SeasonsQuery
            {
                PodcastId = podcastId,
                SeasonId = seasonId
            });
            if (Season == null)
                return NotFound();
        }

        if (!ModelState.IsValid)
            return Page();

        if (!await TryUpdateModelAsync(Season,
                "season",
            s => s.Name,
            s => s.Number))
        {
            return Page();
        }

        await PodcastRepository.AddOrUpdateSeason(Season);
        if (TempData[WellKnownTempData.ErrorMessage] is null)
        {
            TempData[WellKnownTempData.SuccessMessage] = $"Season successfully {(isNew ? "created" : "updated")}.";
        }

        return RedirectToPage("./Index", new { podcastId = Season.PodcastId });
    }
}
