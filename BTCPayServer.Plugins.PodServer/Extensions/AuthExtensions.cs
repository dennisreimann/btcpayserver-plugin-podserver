using BTCPayServer.Plugins.PodServer.Authentication;
using BTCPayServer.Security;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.PodServer.Extensions;

public static class AuthExtensions
{
    public static void AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            foreach (var policy in PodServerPolicies.AllPolicies)
            {
                opts.AddPolicy(policy, policyBuilder => policyBuilder
                    .AddRequirements(new PolicyRequirement(policy)));
            }
        });
    }
}
