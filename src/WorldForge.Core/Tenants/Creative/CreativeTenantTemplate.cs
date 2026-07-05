using WorldForge.Core.Abstractions;
using WorldForge.Core.Enums;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Core.Tenants.Creative;

public sealed class CreativeTenantTemplate : ITenantTemplate
{
    public const string TenantId = "creative";

    public static CreativeTenantTemplate Instance { get; } = new();

    public string Id => TenantId;
    public string DisplayName => "WorldForge";
    public SecurityTier SecurityLevel => SecurityTier.Standard;
    public IReadOnlyList<Type> EntityTypes { get; } =
    [
        typeof(WorldCharacter),
        typeof(WorldLocation),
        typeof(WorldFaction),
        typeof(WorldItem),
        typeof(TimelineEvent),
        typeof(ManuscriptDocument)
    ];
    public IReadOnlyList<string> PluginIds { get; } = [ForgePlugins.WorldBuilding];
    public IReadOnlyList<string> ContradictionRuleIds { get; } = [ForgeRules.AgeTimeline];
    public string SkinId => "immersive-dark";
    public FeatureFlags Features { get; } = new();
}
