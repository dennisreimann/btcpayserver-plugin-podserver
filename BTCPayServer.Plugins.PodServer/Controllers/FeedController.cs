using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Extensions;
using BTCPayServer.Filters;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.PodServer.Controllers;

public class FeedController : PublicBaseController
{
    private readonly IFileService _fileService;

    public FeedController(
        AppService appService,
        PodcastRepository podcastRepository,
        IFileService fileService) : base(appService, podcastRepository)
    {
        _fileService = fileService;
    }

    [ResponseCache(Duration = 1200)]
    [Produces("application/rss+xml")]
    [HttpGet("/plugins/podserver/podcast/{podcastSlug}/feed")]
    public async Task<IActionResult> Feed(string podcastSlug)
    {
        return await FeedResult(new PodcastsQuery { PodcastSlug = podcastSlug });
    }

    [ResponseCache(Duration = 1200)]
    [Produces("application/rss+xml")]
    [HttpGet("/feed")]
    [HttpGet("/apps/{appId}/podcast/feed")]
    [DomainMappingConstraint(PodServerApp.AppType)]
    public async Task<IActionResult> FeedApp(string appId)
    {
        var app = await AppService.GetApp(appId, PodServerApp.AppType);
        if (app == null) return NotFound();

        var settings = app.GetSettings<PodServerSettings>();
        return await FeedResult(new PodcastsQuery { PodcastId = settings.PodcastId });
    }

    // https://hamedfathi.me/a-professional-asp.net-core-rss/
    private async Task<IActionResult> FeedResult(PodcastsQuery podcastsQuery)
    {
        podcastsQuery.IncludePeople = true;
        podcastsQuery.IncludeSeasons = true;
        podcastsQuery.IncludeContributions = true;

        var podcast = await GetPodcast(podcastsQuery);
        if (podcast == null)
            return NotFound();

        var episodes = (await PodcastRepository.GetEpisodes(new EpisodesQuery
        {
            PodcastId = podcast.PodcastId,
            OnlyPublished = true,
            IncludeSeason = true,
            IncludeEnclosures = true,
            IncludeContributions = true
        })).ToList();

        // Setup
        using var stream = new MemoryStream();
        var xml = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            Indent = true,
            Async = true
        });

        var rootUri = Request.GetAbsoluteRootUri();
        var lastUpdated = episodes.FirstOrDefault()?.PublishedAt!.Value ?? DateTimeOffset.Now;

        await xml.WriteStartDocumentAsync();
        xml.WriteStartElement("rss");
        xml.WriteAttributeString("version", "2.0");
        await xml.WriteAttributeStringAsync("xmlns", "itunes", null, "http://www.itunes.com/dtds/podcast-1.0.dtd");
        await xml.WriteAttributeStringAsync("xmlns", "content", null, "http://purl.org/rss/1.0/modules/content/");
        await xml.WriteAttributeStringAsync("xmlns", "podcast", null, "https://podcastindex.org/namespace/1.0");
        await xml.WriteAttributeStringAsync("xmlns", "atom", null, "http://www.w3.org/2005/Atom");
        xml.WriteStartElement("channel");

        // Podcast
        await AddPodcastToXml(podcast, lastUpdated, rootUri, xml);

        // Episodes
        foreach (var episode in episodes)
        {
            await AddEpisodeToXml(podcast, episode, rootUri, xml);
        }

        // End
        await xml.WriteEndElementAsync(); // channel
        await xml.WriteEndElementAsync(); // rss
        await xml.WriteEndDocumentAsync();
        await xml.FlushAsync();

        return File(stream.ToArray(), "application/rss+xml;charset=utf-8", $"{podcast.Slug}.rss");
    }

    private async Task AddPodcastToXml(Podcast podcast, DateTimeOffset lastUpdated, Uri rootUri, XmlWriter xml)
    {
        var imageUrl = string.IsNullOrEmpty(podcast.ImageFileId)
            ? null
            : await _fileService.GetFileUrl(rootUri, podcast.ImageFileId);
        var podcastUrl = string.IsNullOrEmpty(podcast.Url)
            ? Url.ActionLink(nameof(PublicController.Podcast), nameof(PublicController).TrimEnd("Controller", StringComparison.InvariantCulture), new { podcastSlug = podcast.Slug })
            : podcast.Url;

        xml.WriteStartElement("title");
        await xml.WriteCDataAsync(podcast.Title);
        await xml.WriteEndElementAsync();

        xml.WriteStartElement("description");
        await xml.WriteCDataAsync(podcast.Description);
        await xml.WriteEndElementAsync();

        await xml.WriteElementStringAsync(null, "generator", null, "PodServer (BTCPay Server Plugin)");
        await xml.WriteElementStringAsync(null, "language", null, podcast.Language);
        await xml.WriteElementStringAsync(null, "lastBuildDate", null, lastUpdated.ToString("R"));
        await xml.WriteElementStringAsync("podcast", "guid", null, podcast.PodcastId);
        await xml.WriteElementStringAsync("podcast", "medium", null, podcast.Medium);

        await xml.WriteStartElementAsync("atom", "link", null);
        xml.WriteAttributeString("rel", "self");
        xml.WriteAttributeString("type", "application/rss+xml");
        xml.WriteAttributeString("href", Url.ActionLink(nameof(Feed), "Feed", new { podcastSlug = podcast.Slug }));
        await xml.WriteEndElementAsync();

        if (!string.IsNullOrEmpty(podcast.Owner))
        {
            await xml.WriteElementStringAsync(null, "copyright", null, $"{DateTime.Now.Year}, {podcast.Owner}");
            await xml.WriteElementStringAsync("itunes", "author", null, podcast.Owner);
            await xml.WriteStartElementAsync("itunes", "owner", null);
            await xml.WriteElementStringAsync("itunes", "name", null, podcast.Owner);
            if (!string.IsNullOrEmpty(podcast.Email))
            {
                await xml.WriteElementStringAsync("itunes", "email", null, podcast.Email);
            }

            await xml.WriteEndElementAsync();
        }

        if (!string.IsNullOrEmpty(podcast.Category))
        {
            await xml.WriteStartElementAsync("itunes", "category", null);
            xml.WriteAttributeString("text", podcast.Category);
            await xml.WriteEndElementAsync();
        }

        if (!string.IsNullOrEmpty(podcastUrl))
        {
            await xml.WriteElementStringAsync(null, "link", null, podcastUrl);
        }

        if (!string.IsNullOrEmpty(imageUrl))
        {
            xml.WriteStartElement("image");
            await xml.WriteElementStringAsync(null, "url", null, imageUrl);
            await xml.WriteElementStringAsync(null, "title", null, podcast.Title);
            if (!string.IsNullOrEmpty(podcastUrl))
                await xml.WriteElementStringAsync(null, "link", null, podcastUrl);
            await xml.WriteEndElementAsync();

            await xml.WriteStartElementAsync("itunes", "image", null);
            xml.WriteAttributeString("href", imageUrl);
            await xml.WriteEndElementAsync();
        }

        // Value and people
        await AddContributionsToXml(podcast.Contributions, podcast.People, rootUri, xml);
    }

    private async Task AddEpisodeToXml(Podcast podcast, Episode episode, Uri rootUri, XmlWriter xml)
    {
        var enclosure = episode.MainEnclosure;
        var enclosureUrl = await _fileService.GetFileUrl(rootUri, enclosure.FileId);
        var coverUrl = string.IsNullOrEmpty(episode.ImageFileId)
            ? null
            : await _fileService.GetFileUrl(rootUri, episode.ImageFileId);
        var episodeUrl = Url.ActionLink(nameof(PublicController.Episode), nameof(PublicController).TrimEnd("Controller", StringComparison.InvariantCulture),
            new { podcastSlug = podcast.Slug, episodeSlug = episode.Slug }, HttpContext.Request.Scheme);

        xml.WriteStartElement("item");

        await xml.WriteElementStringAsync(null, "pubDate", null, episode.PublishedAt!.Value.ToString("R"));

        xml.WriteStartElement("title");
        await xml.WriteCDataAsync(episode.Title);
        await xml.WriteEndElementAsync();

        xml.WriteStartElement("description");
        await xml.WriteCDataAsync(episode.Description);
        await xml.WriteEndElementAsync();

        await xml.WriteStartElementAsync(null, "enclosure", null);
        xml.WriteAttributeString("url", enclosureUrl);
        xml.WriteAttributeString("type", enclosure.Type);
        xml.WriteAttributeString("length", enclosure.Length.ToString());

        await xml.WriteEndElementAsync();

        if (!string.IsNullOrEmpty(episodeUrl))
        {
            await xml.WriteElementStringAsync(null, "link", null, episodeUrl);
        }

        if (!string.IsNullOrEmpty(coverUrl))
        {
            await xml.WriteStartElementAsync("itunes", "image", null);
            xml.WriteAttributeString("href", coverUrl);
            await xml.WriteEndElementAsync();
        }

        if (episode.Number is > 0)
        {
            await xml.WriteElementStringAsync("podcast", "episode", null, episode.Number.ToString());
        }

        await xml.WriteStartElementAsync(null, "guid", null);
        xml.WriteAttributeString("isPermaLink", "false");
        xml.WriteValue(episode.EpisodeId);
        await xml.WriteEndElementAsync();

        // Value and People
        await AddContributionsToXml(episode.Contributions, podcast.People, rootUri, xml);

        // Season
        if (episode.Season != null)
        {
            await xml.WriteStartElementAsync("podcast", "season", null);
            if (!string.IsNullOrEmpty(episode.Season.Name))
            {
                xml.WriteAttributeString("name", episode.Season.Name);
            }

            xml.WriteValue(episode.Season.Number);
            await xml.WriteEndElementAsync();
        }

        await xml.WriteEndElementAsync();
    }

    private async Task AddContributionsToXml(IEnumerable<Contribution> contributions, IEnumerable<Person> people, Uri rootUri, XmlWriter xml)
    {
        // Value
        await xml.WriteStartElementAsync("podcast", "value", null);
        xml.WriteAttributeString("type", "lightning");
        xml.WriteAttributeString("method", "keysend");

        foreach (var contrib in contributions)
        {
            var person = contrib.Person ?? people.FirstOrDefault(p => p.PersonId == contrib.PersonId);
            if (person.ValueRecipient?.Type == null)
                continue;

            var type = person.ValueRecipient.Type.ToString();
            var address = person.ValueRecipient.Address;
            var split = contrib.Split.ToString();
            var name = string.IsNullOrEmpty(contrib.Role) ? person.Name : $"{person.Name} ({contrib.Role})";

            await xml.WriteStartElementAsync("podcast", "valueRecipient", null);
            xml.WriteAttributeString("name", name);
            xml.WriteAttributeString("type", type);
            xml.WriteAttributeString("address", address);
            xml.WriteAttributeString("split", split);
            await xml.WriteEndElementAsync();
        }

        // PodServer
        await xml.WriteStartElementAsync("podcast", "valueRecipient", null);
        xml.WriteAttributeString("name", "PodServer");
        xml.WriteAttributeString("type", ValueRecipientType.Node.ToString());
        xml.WriteAttributeString("address", "030a58b8653d32b99200a2334cfe913e51dc7d155aa0116c176657a4f1722677a3");
        xml.WriteAttributeString("customKey", "696969");
        xml.WriteAttributeString("customValue", "1M1LbWGzwPE2qViFPG34");
        xml.WriteAttributeString("split", "1");
        xml.WriteAttributeString("fee", "true");
        await xml.WriteEndElementAsync();

        await xml.WriteEndElementAsync();

        // People
        foreach (var contrib in contributions)
        {
            var person = contrib.Person ?? people.FirstOrDefault(p => p.PersonId == contrib.PersonId);
            var avatarUrl = string.IsNullOrEmpty(person.ImageFileId)
                ? null
                : await _fileService.GetFileUrl(rootUri, person.ImageFileId);

            await xml.WriteStartElementAsync("podcast", "person", null);
            if (!string.IsNullOrEmpty(person.Url))
            {
                xml.WriteAttributeString("href", person.Url);
            }

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                xml.WriteAttributeString("img", avatarUrl);
            }

            if (!string.IsNullOrEmpty(contrib.Role))
            {
                xml.WriteAttributeString("role", contrib.Role);
            }

            xml.WriteValue(person.Name);
            await xml.WriteEndElementAsync();
        }
    }
}
