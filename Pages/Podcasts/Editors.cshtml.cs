using System.ComponentModel.DataAnnotations;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthenticationSchemes = BTCPayServer.Abstractions.Constants.AuthenticationSchemes;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanManagePodcast)]
public class EditorsModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    
    [BindProperty]
    public EditorViewModel Editor { get; set; }
    public List<EditorViewModel> Editors { get; set; }
    
    public class EditorViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string UserId { get; set; }
        public EditorRole Role { get; set; } = EditorRole.Editor;
    }

    public EditorsModel(
        UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) {}

    public async Task<IActionResult> OnGetAsync(string podcastId)
    {
        Podcast = await GetPodcast(podcastId);
        if (Podcast == null) return NotFound();

        Editors = await GetEditorVMs(Podcast.Editors);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string podcastId)
    {
        Podcast = await GetPodcast(podcastId);
        if (Podcast == null) return NotFound();

        Editors = await GetEditorVMs(Podcast.Editors);
        if (!ModelState.IsValid) return Page();

        var user = await UserManager.FindByEmailAsync(Editor.Email);
        if (user == null)
        {
            ModelState.AddModelError(nameof(Editor.Email), "User not found");
            return Page();
        }

        if (!Enum.IsDefined(Editor.Role))
        {
            ModelState.AddModelError(nameof(Editor.Role), "Invalid role");
            return Page();
        }
        
        await PodcastRepository.AddOrUpdateEditor(Podcast.PodcastId, user.Id, Editor.Role);
        TempData[WellKnownTempData.SuccessMessage] = "Editor successfully added.";
        return RedirectToPage("./Editors", new { podcastId });
    }

    public async Task<IActionResult> OnPostRemoveAsync(string podcastId, string userId)
    {
        Podcast = await GetPodcast(podcastId);
        if (Podcast == null) return NotFound();

        try
        {
            await PodcastRepository.RemoveEditor(Podcast.PodcastId, userId);
            
            TempData[WellKnownTempData.SuccessMessage] = "Editor successfully removed.";
            return RedirectToPage("./Editors", new { podcastId });
        }
        catch (Exception)
        {
            TempData[WellKnownTempData.ErrorMessage] = "Failed to remove editor.";
        }

        Editors = await GetEditorVMs(Podcast.Editors);
        return Page();
    }

    private async Task<Podcast> GetPodcast(string podcastId)
    {
        return await PodcastRepository.GetPodcast(new PodcastsQuery {
            UserId = UserId,
            PodcastId = podcastId,
            IncludeEditors = true
        });
    }

    private async Task<List<EditorViewModel>> GetEditorVMs(IEnumerable<Editor> editors)
    {
        var list = new List<EditorViewModel>();
        foreach (var editor in editors)
        {
            var user = await UserManager.FindByIdAsync(editor.UserId);
            list.Add(new EditorViewModel
            {
                UserId = editor.UserId,
                Email = user.Email,
                Role = editor.Role
            });
        }
        return list;
    }
}
