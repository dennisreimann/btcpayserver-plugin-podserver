using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTCPayServer.Configuration;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Controllers;
using BTCPayServer.Plugins.PodServer.Data.Models;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using BTCPayServer.Services.Invoices;
using Microsoft.AspNetCore.Routing;

namespace BTCPayServer.Plugins.PodServer;

public class PodServerApp : IApp
{
    public const string AppType = "PodServer";
    private readonly BTCPayServerOptions _options;
    private readonly LinkGenerator _linkGenerator;
    private readonly PodcastRepository _podcastRepository;

    public PodServerApp(
        BTCPayServerOptions options,
        LinkGenerator linkGenerator,
        PodcastRepository podcastRepository)
    {
        _options = options;
        _linkGenerator = linkGenerator;
        _podcastRepository = podcastRepository;
    }

    public string Description => "Podcast";
    public string Type => AppType;
    public bool SupportsSalesStats => false;
    public bool SupportsItemStats => false;

    public async Task<string> ConfigureLink(AppData app)
    {
        var podcast = await PodcastForApp(app);
        return podcast != null
            ? _linkGenerator.GetPathByPage("/Podcasts/Podcast", null, new { podcastId = podcast.PodcastId }, _options.RootPath)
            : _linkGenerator.GetPathByPage("/Podcasts/Create", null, new { appId = app.Id }, _options.RootPath);
    }

    public async Task<string> ViewLink(AppData app)
    {
        var podcast = await PodcastForApp(app);
        return podcast != null
            ? _linkGenerator.GetPathByAction(nameof(PublicController.ViewPodcast), "Public", new { podcastSlug = podcast.Slug }, _options.RootPath)
            : null;
    }

    private async Task<Podcast> PodcastForApp(AppData app)
    {
        var config = app.GetSettings<PodServerSettings>();
        return !string.IsNullOrEmpty(config.PodcastId)
            ? await _podcastRepository.GetPodcast(new PodcastsQuery { PodcastId = config.PodcastId })
            : null;
    }

    public Task<SalesStats> GetSalesStats(AppData app, InvoiceEntity[] paidInvoices, int numberOfDays)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ItemStats>> GetItemStats(AppData appData, InvoiceEntity[] invoiceEntities)
    {
        throw new NotImplementedException();
    }

    public Task<object> GetInfo(AppData app)
    {
        throw new NotImplementedException();
    }

    public Task SetDefaultSettings(AppData appData, string defaultCurrency)
    {
        appData.SetSettings(new PodServerSettings { PodcastId = null });
        return Task.CompletedTask;
    }
}

public class PodServerSettings
{
    public string PodcastId { get; set; }
}
