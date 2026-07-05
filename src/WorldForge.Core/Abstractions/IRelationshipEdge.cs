namespace WorldForge.Core.Abstractions;

/// <summary>
/// 关系边 — CharacterRelationship / AccountBeneficiary / SymptomMedicationLink 共用形态。
/// </summary>
public interface IRelationshipEdge
{
    Guid FromEntityId { get; }
    Guid ToEntityId { get; }
    string RelationType { get; }
}
