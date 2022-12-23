using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Contributions;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManageEpisodes)]
public class EditModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }
    public Contribution Contribution { get; set; }

    public EditModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository, IFileService fileService) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId, string personId, [FromQuery] string episodeId)
    {
        await InitModel(podcastId, personId, episodeId);

        if (Podcast == null)
            return NotFound();
        if (Contribution == null)
            return NotFound();

        if (!Podcast.People.Any())
        {
            TempData[WellKnownTempData.ErrorMessage] = "You need to add a person first, in order to create their contributions.";
            return RedirectToPage("/Person/Create", new { podcastId = Podcast.PodcastId });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string personId, [FromQuery] string episodeId)
    {
        await InitModel(podcastId, personId, episodeId);

        if (Podcast == null)
            return NotFound();
        if (Contribution == null)
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        var isNew = personId == null;

        if (await TryUpdateModelAsync(Contribution,
                "contribution",
                c => c.PersonId,
                c => c.Role,
                c => c.Split))
        {
            await PodcastRepository.AddOrUpdateContribution(Contribution);
            if (TempData[WellKnownTempData.ErrorMessage] is null)
            {
                TempData[WellKnownTempData.SuccessMessage] = $"Contribution successfully {(isNew ? "created" : "updated")}.";
            }

            return RedirectToPage("./Index", new { podcastId = Contribution.PodcastId, episodeId = Contribution.EpisodeId });
        }

        return Page();
    }

    private async Task InitModel(string podcastId, string personId, string episodeId)
    {
        if (string.IsNullOrEmpty(episodeId))
        {
            Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
            {
                UserId = UserId,
                PodcastId = podcastId,
                IncludePeople = true,
                IncludeContributions = true
            });

            Contribution = personId == null
                ? new Contribution { PodcastId = podcastId }
                : Podcast.Contributions.FirstOrDefault(c => c.PersonId == personId);
        }
        else
        {
            Episode = await PodcastRepository.GetEpisode(new EpisodesQuery
            {
                PodcastId = podcastId,
                EpisodeId = episodeId,
                IncludePeople = true,
                IncludeContributions = true
            });
            Podcast = Episode.Podcast;

            Contribution = personId == null
                ? new Contribution { PodcastId = podcastId, EpisodeId = episodeId }
                : Episode.Contributions.FirstOrDefault(c => c.PersonId == personId);
        }
    }
}
