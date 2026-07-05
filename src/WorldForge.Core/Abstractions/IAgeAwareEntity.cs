namespace WorldForge.Core.Abstractions;

/// <summary>
/// 可推导年龄的实体 — 矛盾检测规则的可选目标。
/// </summary>
public interface IAgeAwareEntity : IEntityNode
{
    int? Age { get; }
    DateOnly? BirthDate { get; }
}
