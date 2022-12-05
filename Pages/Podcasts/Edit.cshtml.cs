using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManagePodcast)]
public class EditModel : BasePageModel
{
    private readonly IFileService _fileService;
    public Podcast Podcast { get; set; }
    public IFormFile ImageFile { get; set; }

    public EditModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository, IFileService fileService) : base(userManager, podcastRepository)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGet(string podcastId)
    {
        Podcast = await GetPodcast(podcastId);
        if (Podcast == null) return NotFound();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string podcastId)
    {
        Podcast = await GetPodcast(podcastId);
        if (Podcast == null) return NotFound();
        
        if (!ModelState.IsValid) return Page();
        
        if (ImageFile != null)
        {
            // delete existing image
            if (!string.IsNullOrEmpty(Podcast.ImageFileId))
            {
                await _fileService.RemoveFile(Podcast.ImageFileId, UserId);
            }
            // add new image
            try
            {
                var storedFile = await _fileService.AddFile(ImageFile, UserId);
                Podcast.ImageFileId = storedFile.Id;
            }
            catch (Exception e)
            {
                TempData[WellKnownTempData.ErrorMessage] = $"Could not save image: {e.Message}";
            }
        }

        if (!await TryUpdateModelAsync(
                Podcast, 
                "podcast",
                p => p.Title,
                p => p.Description,
                p => p.Medium,
                p => p.Language,
                p => p.Category,
                p => p.Owner,
                p => p.Email,
                p => p.Url,
                p => p.Slug,
                p => p.ImageFileId))
        {
            return Page();
        }
        
        await PodcastRepository.AddOrUpdatePodcast(Podcast);
        if (TempData[WellKnownTempData.ErrorMessage] is null)
        {
            TempData[WellKnownTempData.SuccessMessage] = "Podcast successfully updated.";
        }
        
        return RedirectToPage("./Podcast", new { podcastId = Podcast.PodcastId });
    }

    private async Task<Podcast> GetPodcast(string podcastId)
    {
        return await PodcastRepository.GetPodcast(new PodcastsQuery {
            UserId = UserId,
            PodcastId = podcastId
        });
    }
}
