using WorldForge.Core.Enums;
using WorldForge.Core.Tenants.Legacy.Entities;

namespace WorldForge.Core.Tenants.Legacy;

/// <summary>
/// B 场景占位租户 — LegacyVault（月 12–18），E2EE Phase 3。
/// </summary>
public sealed class LegacyTenantTemplate : ITenantTemplate
{
    public const string TenantId = "legacy";

    public static LegacyTenantTemplate Instance { get; } = new();

    public string Id => TenantId;
    public string DisplayName => "LegacyVault";
    public SecurityTier SecurityLevel => SecurityTier.E2EE;
    public IReadOnlyList<Type> EntityTypes { get; } =
    [
        typeof(DigitalAccount),
        typeof(LifeEvent),
        typeof(Letter)
    ];
    public IReadOnlyList<string> PluginIds { get; } = [ForgePlugins.LegacyCompliance];
    public IReadOnlyList<string> ContradictionRuleIds { get; } = [];
    public string SkinId => "trust-neutral";
    public FeatureFlags Features { get; } = new(ContradictionDetection: false);
}
