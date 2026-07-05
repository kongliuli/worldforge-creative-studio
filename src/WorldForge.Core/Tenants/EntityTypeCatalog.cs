namespace WorldForge.Core.Tenants;

/// <summary>
/// 全租户 Note 子类型注册表 — DbContext TPH 从此读取，新增垂直场景只注册不改编排。
/// </summary>
public static class EntityTypeCatalog
{
    private static readonly Dictionary<string, Type> Discriminators = new(StringComparer.OrdinalIgnoreCase);
    private static bool _initialized;

    public static IReadOnlyDictionary<string, Type> All => Discriminators;

    public static void RegisterFromTenants(IEnumerable<ITenantTemplate> tenants)
    {
        foreach (var tenant in tenants)
        {
            foreach (var entityType in tenant.EntityTypes)
            {
                var name = entityType.Name;
                Discriminators[name] = entityType;
            }
        }
        _initialized = true;
    }

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        RegisterFromTenants([
            Creative.CreativeTenantTemplate.Instance,
            Legacy.LegacyTenantTemplate.Instance,
            Health.HealthTenantTemplate.Instance
        ]);
    }
}
