using WorldForge.Core.Entities;
using WorldForge.Core.Enums;

namespace WorldForge.Core.Tenants.Creative.Entities;

public class ManuscriptDocument : Note
{
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public int WordCount { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public ICollection<EntityLink> EntityLinks { get; set; } = [];
}
