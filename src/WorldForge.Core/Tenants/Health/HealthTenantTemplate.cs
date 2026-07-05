using WorldForge.Core.Enums;
using WorldForge.Core.Tenants.Health.Entities;

namespace WorldForge.Core.Tenants.Health;

/// <summary>
/// C 场景占位租户 — PMF 后启用，实体已注册 TPH 便于引擎复用验证。
/// </summary>
public sealed class HealthTenantTemplate : ITenantTemplate
{
    public const string TenantId = "health";

    public static HealthTenantTemplate Instance { get; } = new();

    public string Id => TenantId;
    public string DisplayName => "HealthVault";
    public SecurityTier SecurityLevel => SecurityTier.Enhanced;
    public IReadOnlyList<Type> EntityTypes { get; } =
    [
        typeof(SymptomRecord),
        typeof(DoctorVisit),
        typeof(HealthNote)
    ];
    public IReadOnlyList<string> PluginIds { get; } = [ForgePlugins.HealthSymptomCheck];
    public IReadOnlyList<string> ContradictionRuleIds { get; } = [];
    public string SkinId => "clinical-light";
    public FeatureFlags Features { get; } = new(ContradictionDetection: false);
}
