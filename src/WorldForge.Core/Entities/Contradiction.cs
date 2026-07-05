using WorldForge.Core.Enums;

namespace WorldForge.Core.Entities;

public class Contradiction
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ContradictionType Type { get; set; }
    public ContradictionSeverity Severity { get; set; }
    public string Summary { get; set; } = "";
    public Guid? SourceDocumentId { get; set; }
    public Guid? TargetDocumentId { get; set; }
    public string SourceExcerpt { get; set; } = "";
    public string TargetExcerpt { get; set; } = "";
    public Guid? RelatedEntityId { get; set; }
    public string? Suggestion { get; set; }
    public ContradictionStatus Status { get; set; } = ContradictionStatus.Open;
    public DateTime DetectedAt { get; set; }
    public Guid DetectionRunId { get; set; }

    public Project Project { get; set; } = null!;
}
