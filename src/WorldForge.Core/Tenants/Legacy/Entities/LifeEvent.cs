using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Legacy.Entities;

/// <summary>映射文档：TimelineEvent → LifeEvent</summary>
public class LifeEvent : Note, ITimelineEntry, IEntityNode
{
    public DateOnly? EventDate { get; set; }
    public long SortKey { get; set; }
    public List<Guid> RelatedEntityIds { get; set; } = [];

    IReadOnlyList<Guid> ITimelineEntry.RelatedEntityIds => RelatedEntityIds;
    public string DisplayName => Title;
}
