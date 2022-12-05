namespace BTCPayServer.Plugins.PodServer.Services.Podcasts;

public class PodcastsQuery
{
    public string UserId { get; set; }
    public string PodcastId { get; set; }
    public string Slug { get; set; }
    public bool IncludeEpisodes { get; set; }
    public bool IncludeSeasons { get; set; }
    public bool IncludePeople { get; set; }
    public bool IncludeImports { get; set; }
    public bool IncludeContributions { get; set; }
    public bool IncludeEditors { get; set; }
}
