using System.Collections.Generic;
using BTCPayServer.Plugins.PodServer.Data.Models;

namespace BTCPayServer.Plugins.PodServer.ViewModels;

public class PublicPodcastViewModel
{
    public Podcast Podcast { get; set; }
    public Episode LatestEpisode { get; set; }
    public IEnumerable<Episode> MoreEpisodes { get; set; }
}
