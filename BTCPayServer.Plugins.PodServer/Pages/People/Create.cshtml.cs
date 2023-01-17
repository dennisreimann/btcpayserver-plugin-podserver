using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.People;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManagePodcast)]
public class CreateModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public Person Person { get; set; }

    public CreateModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId
        });
        if (Podcast == null)
            return NotFound();

        Person = new Person
        {
            PodcastId = Podcast.PodcastId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId)
    {
        if (!ModelState.IsValid)
            return Page();

        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery { UserId = UserId, PodcastId = podcastId });

        Person = new Person
        {
            PodcastId = Podcast.PodcastId
        };

        if (await TryUpdateModelAsync(
            Person,
            "person",
            p => p.Name))
        {
            await PodcastRepository.AddOrUpdatePerson(Person);

            TempData[WellKnownTempData.SuccessMessage] = "Person successfully created.";
            return RedirectToPage("./Edit", new { podcastId = Podcast.PodcastId, personId = Person.PersonId });
        }

        return Page();
    }
}
