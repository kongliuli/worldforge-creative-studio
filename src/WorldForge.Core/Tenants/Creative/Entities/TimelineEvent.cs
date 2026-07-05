using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Creative.Entities;

public class TimelineEvent : Note, ITimelineEntry, IEntityNode
{
    public DateOnly? EventDate { get; set; }
    public string? EraName { get; set; }
    public long SortKey { get; set; }
    public List<Guid> RelatedEntityIds { get; set; } = [];

    IReadOnlyList<Guid> ITimelineEntry.RelatedEntityIds => RelatedEntityIds;
    public string DisplayName => Title;
}
