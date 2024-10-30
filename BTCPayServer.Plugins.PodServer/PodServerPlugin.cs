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
    public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
    [
        new () { Identifier = nameof(BTCPayServer), Condition = ">=2.0.0" }
    ];

    public override void Execute(IServiceCollection services)
    {
        services.AddSingleton<AppBaseType, PodServerApp>();
        services.AddUIExtension("header-nav", "PodServerNavExtension");
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
