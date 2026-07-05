using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Health.Entities;

/// <summary>映射文档：TimelineEvent → DoctorVisit</summary>
public class DoctorVisit : Note, ITimelineEntry, IEntityNode
{
    public DateOnly? EventDate { get; set; }
    public long SortKey { get; set; }
    public List<Guid> RelatedEntityIds { get; set; } = [];

    IReadOnlyList<Guid> ITimelineEntry.RelatedEntityIds => RelatedEntityIds;
    public string DisplayName => Title;
}
