namespace BTCPayServer.Plugins.PodServer.Services.Podcasts;

public class ContributionsQuery
{
    public string PodcastId { get; set; }
    public string EpisodeId { get; set; }
    public string PersonId { get; set; }
    public bool PodcastOnly { get; set; }
    
    public bool IncludePerson { get; set; }
}
