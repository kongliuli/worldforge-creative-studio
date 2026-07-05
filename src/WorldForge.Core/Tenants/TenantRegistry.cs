using WorldForge.Core.Abstractions;
using WorldForge.Core.Enums;
using WorldForge.Core.Tenants.Creative;
using WorldForge.Core.Tenants.Health;
using WorldForge.Core.Tenants.Legacy;

namespace WorldForge.Core.Tenants;

public sealed class TenantRegistry : ITenantRegistry
{
    private readonly Dictionary<string, ITenantTemplate> _templates;

    public TenantRegistry(IEnumerable<ITenantTemplate> templates)
    {
        _templates = templates.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ITenantTemplate> All => _templates.Values.ToList();

    public ITenantTemplate Get(string tenantId) =>
        _templates.GetValueOrDefault(tenantId) ?? CreativeTenantTemplate.Instance;

    public ITenantTemplate GetRequired(string tenantId) =>
        _templates.TryGetValue(tenantId, out var template)
            ? template
            : throw new KeyNotFoundException($"Unknown tenant: {tenantId}");
}

public sealed class TenantContext(ITenantRegistry registry) : ITenantContext
{
    private ITenantTemplate _current = registry.GetRequired(CreativeTenantTemplate.TenantId);

    public ITenantTemplate Current => _current;

    public void SetTenant(string tenantId) =>
        _current = registry.GetRequired(tenantId);
}

public static class TenantIds
{
    public const string Creative = CreativeTenantTemplate.TenantId;
    public const string Legacy = LegacyTenantTemplate.TenantId;
    public const string Health = HealthTenantTemplate.TenantId;
}

public static class ForgePlugins
{
    public const string WorldBuilding = "world-building";
    public const string LegacyCompliance = "legacy-compliance";
    public const string HealthSymptomCheck = "health-symptom-check";
}

public static class ForgeRules
{
    public const string AgeTimeline = "age-timeline";
}
