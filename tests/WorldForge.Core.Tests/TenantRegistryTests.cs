using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative;
using WorldForge.Core.Tenants.Health;
using WorldForge.Core.Tenants.Legacy;

namespace WorldForge.Core.Tests;

public class TenantRegistryTests
{
    private readonly TenantRegistry _registry = new([
        CreativeTenantTemplate.Instance,
        LegacyTenantTemplate.Instance,
        HealthTenantTemplate.Instance
    ]);

    [Fact]
    public void All_ContainsThreeTenants() =>
        Assert.Equal(3, _registry.All.Count);

    [Fact]
    public void EntityTypeCatalog_RegistersAllTenantEntities()
    {
        EntityTypeCatalog.RegisterFromTenants(_registry.All);
        Assert.Equal(12, EntityTypeCatalog.All.Count);
    }

    [Fact]
    public void LegacyTenant_HasE2EESecurityTier() =>
        Assert.Equal(Enums.SecurityTier.E2EE, _registry.GetRequired(TenantIds.Legacy).SecurityLevel);

    [Fact]
    public void HealthTenant_DisablesContradictionDetection() =>
        Assert.False(_registry.GetRequired(TenantIds.Health).Features.ContradictionDetection);
}
