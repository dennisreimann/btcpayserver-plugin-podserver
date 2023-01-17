using System.Collections.Generic;
using System.Linq;
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

[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie, Policy = PodServerPolicies.CanView)]
public class IndexModel : BasePageModel
{
    public Podcast Podcast { get; set; }
    public IEnumerable<Person> People { get; set; }

    public IndexModel(UserManager<ApplicationUser> userManager,
        PodcastRepository podcastRepository) : base(userManager, podcastRepository) { }

    public async Task<IActionResult> OnGet(string podcastId)
    {
        Podcast = await PodcastRepository.GetPodcast(new PodcastsQuery
        {
            UserId = UserId,
            PodcastId = podcastId,
            IncludePeople = true
        });

        People = Podcast.People.OrderBy(p => p.Name);

        return Page();
    }
}
