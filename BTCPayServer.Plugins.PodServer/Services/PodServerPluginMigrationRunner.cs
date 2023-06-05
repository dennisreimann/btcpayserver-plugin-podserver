using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Data;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using BTCPayServer.Services.Apps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BTCPayServer.Plugins.PodServer.Services;

public class PodServerPluginMigrationRunner : IHostedService
{
    private readonly PodServerPluginDbContextFactory _dbContextFactory;
    private readonly ISettingsRepository _settingsRepository;
    private readonly PodcastRepository _podcastRepository;
    private readonly AppService _appService;

    public PodServerPluginMigrationRunner(
        PodServerPluginDbContextFactory dbContextFactory,
        ISettingsRepository settingsRepository,
        PodcastRepository podcastRepository,
        AppService appService)
    {
        _dbContextFactory = dbContextFactory;
        _settingsRepository = settingsRepository;
        _podcastRepository = podcastRepository;
        _appService = appService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings =
            await _settingsRepository.GetSettingAsync<PodServerPluginDataMigrationHistory>() ??
            new PodServerPluginDataMigrationHistory();

        await using var ctx = _dbContextFactory.CreateContext();
        await using var dbContext = _dbContextFactory.CreateContext();
        await ctx.Database.MigrateAsync(cancellationToken);

        if (!settings.InitialAppUpdates)
        {
            var podServerApps = await _appService.GetApps(PodServerApp.AppType);
            var podcasts = (await _podcastRepository.GetPodcasts(new PodcastsQuery())).ToList();

            // podcasts without apps
            var podserverSettings = podServerApps
                .Select(appData => appData.GetSettings<PodServerSettings>().PodcastId);
            var podcastsWithoutApps = podcasts
                .Where(p => !podserverSettings.Contains(p.PodcastId));
            foreach (var podcast in podcastsWithoutApps)
            {
                var appData = new AppData
                {
                    Name = podcast.Title,
                    AppType = PodServerApp.AppType
                };
                appData.SetSettings(new PodServerSettings { PodcastId = podcast.PodcastId });
                await _appService.UpdateOrCreateApp(appData);
            }

            // apps without podcasts
            var appsWithoutPodcasts = podServerApps
                .Where(appData =>
                {
                    var appSettings = appData.GetSettings<PodServerSettings>();
                    return string.IsNullOrEmpty(appSettings.PodcastId) || podcasts.All(p => p.PodcastId != appSettings.PodcastId);
                });
            foreach (var app in appsWithoutPodcasts)
            {
                await _appService.DeleteApp(app);
            }

            settings.InitialAppUpdates = true;
            await _settingsRepository.UpdateSetting(settings);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private class PodServerPluginDataMigrationHistory
    {
        public bool InitialAppUpdates { get; set; }
    }
}
