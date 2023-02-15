using System;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using BTCPayServer.Plugins.PodServer.Data;
using BTCPayServer.Plugins.PodServer.Extensions;
using BTCPayServer.Plugins.PodServer.Hooks;
using BTCPayServer.Plugins.PodServer.Services;
using BTCPayServer.Services.Apps;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.PodServer;

public class PodServerPlugin : BaseBTCPayServerPlugin
{
    public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } = new[]
    {
        new IBTCPayServerPlugin.PluginDependency { Identifier = nameof(BTCPayServer), Condition = ">=1.8.2" }
    };

    public override void Execute(IServiceCollection services)
    {
        services.AddSingleton<IApp, PodServerApp>();
        services.AddSingleton<IUIExtension>(new UIExtension("PodServerNavExtension", "header-nav"));
        services.AddSingleton<PodServerPluginDbContextFactory>();
        services.AddDbContext<PodServerPluginDbContext>((provider, o) =>
        {
            var factory = provider.GetRequiredService<PodServerPluginDbContextFactory>();
            factory.ConfigureBuilder(o);
        });

        services.AddSingleton<IPluginHookFilter, AuthorizationRequirementHandler>();

        services.AddAppServices();
        services.AddAppAuthorization();
    }

    public override void Execute(IApplicationBuilder applicationBuilder, IServiceProvider applicationBuilderApplicationServices)
    {
        base.Execute(applicationBuilder, applicationBuilderApplicationServices);
        applicationBuilderApplicationServices.GetService<PodServerPluginDbContextFactory>()?.CreateContext().Database.Migrate();
    }
}
