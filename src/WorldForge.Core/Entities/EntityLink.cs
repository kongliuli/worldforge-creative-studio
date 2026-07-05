using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Core.Entities;

public class EntityLink
{
    public Guid Id { get; set; }
    public Guid SourceDocumentId { get; set; }
    public Guid TargetEntityId { get; set; }
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }

    public ManuscriptDocument SourceDocument { get; set; } = null!;
    public Note TargetEntity { get; set; } = null!;
}
