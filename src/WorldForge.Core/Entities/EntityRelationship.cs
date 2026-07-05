using WorldForge.Core.Abstractions;

namespace WorldForge.Core.Entities;

/// <summary>
/// 通用关系边 — Creative / Legacy / Health 共用，避免每场景重复 ORM 映射。
/// </summary>
public class EntityRelationship : IRelationshipEdge
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FromEntityId { get; set; }
    public Guid ToEntityId { get; set; }
    public string RelationType { get; set; } = "";
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
}
