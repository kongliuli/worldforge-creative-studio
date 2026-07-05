namespace WorldForge.Core.Abstractions;

/// <summary>
/// 时间线条目 — Creative TimelineEvent、Legacy LifeEvent、Health DoctorVisit 共用。
/// </summary>
public interface ITimelineEntry
{
    Guid Id { get; }
    string Title { get; }
    DateOnly? EventDate { get; }
    long SortKey { get; }
    IReadOnlyList<Guid> RelatedEntityIds { get; }
}
