namespace BTCPayServer.Plugins.PodServer.Services.Podcasts;

public class PeopleQuery
{
    public string PodcastId { get; set; }
    public string PersonId { get; set; }
    public string Name { get; set; }
    public bool IncludeContributions { get; set; }
}
