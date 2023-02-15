using System;
using System.IO;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Client;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Extensions;
using BTCPayServer.Plugins.PodServer.Services.Imports;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = Policies.CanViewProfile)]
public class CreateModel : BasePageModel
{
    private readonly AppService _appService;

    public Podcast Podcast { get; set; }

    [BindProperty(SupportsGet = true)]
    public string AppId { get; set; }

    public CreateModel(
        UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository,
        AppService appService) : base(userManager, podcastRepository)
    {
        _appService = appService;
    }

    public async Task<IActionResult> OnGet()
    {
        Podcast = new Podcast();

        if (!string.IsNullOrEmpty(AppId))
        {
            var appData = await _appService.GetApp(AppId, PodServerApp.AppType);
            Podcast.Title = appData.Name;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        Podcast = new Podcast { OwnerId = UserId };

        if (await TryUpdateModelAsync(
            Podcast,
            "podcast",
            p => p.Title,
            p => p.Description,
            p => p.Medium,
            p => p.Language,
            p => p.Category))
        {
            Podcast.Slug = Podcast.Title.Slugify();

            await PodcastRepository.AddOrUpdatePodcast(Podcast);
            await PodcastRepository.AddOrUpdateEditor(Podcast.PodcastId, UserId, EditorRole.Admin);

            if (!string.IsNullOrEmpty(AppId))
            {
                await AssociateApp(Podcast.PodcastId, AppId);
            }

            TempData[WellKnownTempData.SuccessMessage] = "Podcast successfully created.";
            return RedirectToPage("./Podcast", new { podcastId = Podcast.PodcastId });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostImportAsync([FromForm] IFormFile rssFile, [FromServices] FeedImporter importer)
    {
        try
        {
            if (!rssFile.ContentType.EndsWith("xml"))
            {
                throw new Exception($"Invalid RSS file: Content type {rssFile.ContentType} does not match XML.");
            }

            using var reader = new StreamReader(rssFile.OpenReadStream());
            var rss = await reader.ReadToEndAsync();

            Podcast = await importer.CreatePodcast(rss, UserId);

            if (!string.IsNullOrEmpty(AppId))
            {
                await AssociateApp(Podcast.PodcastId, AppId);
            }

            TempData[WellKnownTempData.SuccessMessage] = "Podcast successfully created. The feed is now being imported and the progress will be shown here.";
            return RedirectToPage("./Podcast", new { podcastId = Podcast.PodcastId });
        }
        catch (Exception ex)
        {
            TempData[WellKnownTempData.ErrorMessage] = $"Import failed: {ex.Message}" +
                                                       (!string.IsNullOrEmpty(ex.InnerException?.Message) ? $" ({ex.InnerException.Message})" : "");
            return Page();
        }
    }

    private async Task AssociateApp(string podcastId, string appId)
    {
        var appData = await _appService.GetApp(appId, PodServerApp.AppType);
        var settings = appData.GetSettings<PodServerSettings>();
        settings.PodcastId = podcastId;
        appData.SetSettings(settings);
        await _appService.UpdateOrCreateApp(appData);;
    }
}
