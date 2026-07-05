using Microsoft.Extensions.DependencyInjection;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Rules;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative;
using WorldForge.Core.Tenants.Health;
using WorldForge.Core.Tenants.Legacy;

namespace WorldForge.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddWorldForgeCore(this IServiceCollection services)
    {
        EntityTypeCatalog.RegisterFromTenants([
            CreativeTenantTemplate.Instance,
            LegacyTenantTemplate.Instance,
            HealthTenantTemplate.Instance
        ]);

        services.AddSingleton<ITenantRegistry>(sp =>
            new TenantRegistry([
                CreativeTenantTemplate.Instance,
                LegacyTenantTemplate.Instance,
                HealthTenantTemplate.Instance
            ]));
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<IContradictionRule, AgeTimelineContradictionRule>();

        return services;
    }
}
