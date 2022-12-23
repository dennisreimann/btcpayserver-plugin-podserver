using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using BTCPayServer.Plugins.PodServer.Data;
using BTCPayServer.Plugins.PodServer.Extensions;
using BTCPayServer.Plugins.PodServer.Hooks;
using BTCPayServer.Plugins.PodServer.Services;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.PodServer
{
    public class PodServerPlugin : BaseBTCPayServerPlugin
    {
        public override string Name { get; } = "PodServer";
        public override string Identifier { get; } = "BTCPayServer.Plugins.PodServer";
        public override string Description { get; } = "Self-host your podcast and start Podcasting 2.0 like a pro.";
        public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } = new[]
        {
            new IBTCPayServerPlugin.PluginDependency
            {
                Identifier = nameof(BTCPayServer),
                Condition = ">=1.7.1.0"
            }
        };

        public override void Execute(IServiceCollection services)
        {
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
}
