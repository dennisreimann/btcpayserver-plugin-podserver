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
public class DeleteModel : BasePageModel
{
    public Person Person { get; set; }

    public DeleteModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId, string personId)
    {
        Person = await GetPerson(podcastId, personId);
        if (Person == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string personId)
    {
        Person = await GetPerson(podcastId, personId);
        if (Person == null)
            return NotFound();

        await PodcastRepository.RemovePerson(Person);
        TempData[WellKnownTempData.SuccessMessage] = "Person successfully removed.";

        return RedirectToPage("./Index", new { podcastId = Person.PodcastId });
    }

    private async Task<Person> GetPerson(string podcastId, string personId)
    {
        return await PodcastRepository.GetPerson(new PeopleQuery
        {
            PodcastId = podcastId,
            PersonId = personId,
            IncludeContributions = true
        });
    }
}
