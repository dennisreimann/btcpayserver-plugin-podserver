using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.People;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManagePodcast)]
public class EditModel : BasePageModel
{
    private readonly IFileService _fileService;
    public Person Person { get; set; }
    public IFormFile ImageFile { get; set; }

    public EditModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository, IFileService fileService) : base(userManager, podcastRepository)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGet(string podcastId, string personId)
    {
        Person = await GetPerson(podcastId, personId);
        if (Person == null) return NotFound();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId, string personId)
    {
        Person = await GetPerson(podcastId, personId);
        if (Person == null) return NotFound();

        if (!ModelState.IsValid) return Page();
        
        if (ImageFile != null)
        {
            // delete existing image
            if (!string.IsNullOrEmpty(Person.ImageFileId))
            {
                await _fileService.RemoveFile(Person.ImageFileId, UserId);
            }
            // add new image
            try
            {
                var storedFile = await _fileService.AddFile(ImageFile, UserId);
                Person.ImageFileId = storedFile.Id;
            }
            catch (Exception e)
            {
                TempData[WellKnownTempData.ErrorMessage] = $"Could not save image: {e.Message}";
            }
        }
        
        if (await TryUpdateModelAsync(Person,
                "person",
            p => p.Name,
            p => p.Url,
            p => p.ImageFileId,
                p => p.ValueRecipient))
        {
            await PodcastRepository.AddOrUpdatePerson(Person);
            if (TempData[WellKnownTempData.ErrorMessage] is null)
            {
                TempData[WellKnownTempData.SuccessMessage] = "Person successfully updated.";
            }
        
            return RedirectToPage("./Index", new { podcastId = Person.PodcastId });
        }
        
        return Page();
    }
    
    private async Task<Person> GetPerson(string podcastId, string personId)
    {
        return await PodcastRepository.GetPerson(new PeopleQuery {
            PodcastId = podcastId,
            PersonId = personId
        });
    }
}
