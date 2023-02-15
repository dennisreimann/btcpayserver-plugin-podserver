using System.Collections.Generic;
using BTCPayServer.Plugins.PodServer.Data.Models;

namespace BTCPayServer.Plugins.PodServer.ViewModels;

public class PublicEpisodeViewModel
{
    public Podcast Podcast { get; set; }
    public Episode Episode { get; set; }
}
