namespace WorldForge.Core.Tenants;

public record FeatureFlags(
    bool ContradictionDetection = true,
    bool Collaboration = false,
    bool CloudSync = false);

/// <summary>
/// 租户模板 — 引擎通过此配置注入场景差异，避免分支代码。
/// </summary>
public interface ITenantTemplate
{
    string Id { get; }
    string DisplayName { get; }
    Enums.SecurityTier SecurityLevel { get; }
    IReadOnlyList<Type> EntityTypes { get; }
    IReadOnlyList<string> PluginIds { get; }
    IReadOnlyList<string> ContradictionRuleIds { get; }
    string SkinId { get; }
    FeatureFlags Features { get; }
}

public interface ITenantRegistry
{
    ITenantTemplate Get(string tenantId);
    ITenantTemplate GetRequired(string tenantId);
    IReadOnlyList<ITenantTemplate> All { get; }
}

public interface ITenantContext
{
    ITenantTemplate Current { get; }
    void SetTenant(string tenantId);
}
