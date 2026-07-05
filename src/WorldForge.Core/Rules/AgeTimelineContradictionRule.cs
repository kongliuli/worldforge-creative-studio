using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Enums;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Core.Rules;

/// <summary>
/// 年龄 vs 时间线 — Creative 租户规则，通过接口注册可复用到其他 IAgeAwareEntity 场景。
/// </summary>
public sealed class AgeTimelineContradictionRule : IContradictionRule
{
    public string RuleId => ForgeRules.AgeTimeline;
    public IReadOnlyList<string> SupportedTenantIds { get; } = [TenantIds.Creative];

    public IEnumerable<Contradiction> Evaluate(ContradictionRuleContext context)
    {
        var characters = context.Notes.OfType<WorldCharacter>().Cast<IAgeAwareEntity>().ToList();
        var events = context.Notes.OfType<TimelineEvent>().Cast<ITimelineEntry>().ToList();

        foreach (var character in characters)
        {
            var worldCharacter = (WorldCharacter)character;
            foreach (var timelineEvent in events)
            {
                if (timelineEvent.RelatedEntityIds.Count > 0 &&
                    !timelineEvent.RelatedEntityIds.Contains(character.Id))
                    continue;

                foreach (var contradiction in EvaluatePair(
                             worldCharacter,
                             (TimelineEvent)timelineEvent,
                             context.ProjectId,
                             context.DetectionRunId))
                    yield return contradiction;
            }
        }
    }

    public static IEnumerable<Contradiction> EvaluatePair(
        WorldCharacter character,
        TimelineEvent timelineEvent,
        Guid projectId,
        Guid detectionRunId)
    {
        if (character.BirthDate is null || timelineEvent.EventDate is null || character.Age is null)
            yield break;

        var expectedAge = timelineEvent.EventDate.Value.Year - character.BirthDate.Value.Year;
        if (timelineEvent.EventDate.Value < character.BirthDate.Value.AddYears(expectedAge))
            expectedAge--;

        if (expectedAge == character.Age.Value)
            yield break;

        yield return new Contradiction
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Type = ContradictionType.AgeMismatch,
            Severity = ContradictionSeverity.Error,
            Summary = $"角色「{character.FullName}」年龄 {character.Age} 与时间线事件「{timelineEvent.Title}」推导年龄 {expectedAge} 不一致",
            RelatedEntityId = character.Id,
            Suggestion = $"将角色年龄改为 {expectedAge}，或调整出生日期/事件日期",
            Status = ContradictionStatus.Open,
            DetectedAt = DateTime.UtcNow,
            DetectionRunId = detectionRunId
        };
    }
}
