using WorldForge.Core.Entities;

namespace WorldForge.Core.Abstractions;

public record ContradictionDetectionRequest(
    Guid ProjectId,
    Guid? FocusDocumentId,
    IReadOnlyList<Note> Notes,
    IReadOnlyList<EntityLink> Links,
    string PreferredOllamaModel = "phi4-mini");

public record ContradictionFinding(
    string Type,
    string Severity,
    string Summary,
    Guid? SourceDocumentId,
    string SourceExcerpt,
    Guid? TargetDocumentId,
    string TargetExcerpt,
    Guid? RelatedEntityId,
    string? Suggestion);

/// <summary>
/// AI 层矛盾检测插件 — WorldBuildingPlugin 等实现。
/// </summary>
public interface IContradictionDetectionPlugin : IWorldForgePlugin
{
    Task<IReadOnlyList<ContradictionFinding>> DetectAsync(
        ContradictionDetectionRequest request,
        CancellationToken ct = default);
}
