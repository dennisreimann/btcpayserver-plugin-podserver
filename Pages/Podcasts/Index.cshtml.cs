using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Client;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Pages.Podcasts;

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = Policies.CanViewProfile)]
public class IndexModel : BasePageModel
{
    private readonly IFileService _fileService;
    public IEnumerable<Podcast> Podcasts { get; set; }
    public bool IsReady { get; set; }

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository,
        IFileService fileService) : base(userManager, podcastRepository)
    {
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGet()
    {

        Podcasts = await PodcastRepository.GetPodcasts(new PodcastsQuery
        {
            UserId = UserId
        });

        IsReady = await _fileService.IsAvailable();

        if (IsReady)
        {
            var list = Podcasts.ToList();
            if (!list.Any())
            {
                return RedirectToPage("./Create");
            }
        }

        return Page();
    }
}
