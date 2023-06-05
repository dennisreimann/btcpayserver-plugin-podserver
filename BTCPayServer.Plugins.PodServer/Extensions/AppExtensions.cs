using BTCPayServer.Plugins.PodServer.Services;
using BTCPayServer.Plugins.PodServer.Services.Background;
using BTCPayServer.Plugins.PodServer.Services.Imports;
using BTCPayServer.Plugins.PodServer.Services.Podcasts;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.PodServer.Extensions;

public static class AppExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddHostedService<PodServerPluginMigrationRunner>();
        services.AddHostedService<TaskQueueService>();
        services.AddSingleton<PodcastRepository>();
        services.AddSingleton<ImportRepository>();
        services.AddSingleton<FeedImporter>();
        services.AddSingleton<ITaskQueue>(ctx => new TaskQueue(10));
    }
}
