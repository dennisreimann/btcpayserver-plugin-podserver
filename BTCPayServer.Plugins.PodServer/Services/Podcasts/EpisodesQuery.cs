namespace BTCPayServer.Plugins.PodServer.Services.Podcasts;

public class EpisodesQuery
{
    public string PodcastId { get; set; }
    public string EpisodeId { get; set; }
    public string SeasonId { get; set; }
    public string Slug { get; set; }
    public string ImportGuid { get; set; }
    public bool IncludePodcast { get; set; }
    public bool IncludePeople { get; set; }
    public bool IncludeSeason { get; set; }
    public bool IncludeEnclosures { get; set; }
    public bool IncludeContributions { get; set; }
    public bool OnlyPublished { get; set; }
}
