namespace WorldForge.Core.Abstractions;

/// <summary>
/// 实体图谱节点 — Creative 角色、Legacy 账户、Health 症状等共用。
/// </summary>
public interface IEntityNode
{
    Guid Id { get; }
    string DisplayName { get; }
}
