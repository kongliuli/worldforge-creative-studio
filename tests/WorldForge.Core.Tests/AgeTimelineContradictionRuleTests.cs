using WorldForge.Core.Enums;
using WorldForge.Core.Rules;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Core.Tests;

public class AgeTimelineContradictionRuleTests
{
    private readonly AgeTimelineContradictionRule _rule = new();

    [Fact]
    public void Evaluate_WhenAgeMismatch_ReturnsContradiction()
    {
        var character = new WorldCharacter
        {
            Id = Guid.NewGuid(),
            FullName = "Aldric",
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 25
        };
        var timelineEvent = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            Title = "Battle",
            EventDate = new DateOnly(2024, 6, 1),
            RelatedEntityIds = [character.Id]
        };
        var runId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var results = AgeTimelineContradictionRule.EvaluatePair(
            character, timelineEvent, projectId, runId).ToList();

        Assert.Single(results);
        Assert.Equal(ContradictionType.AgeMismatch, results[0].Type);
        Assert.Equal(ContradictionSeverity.Error, results[0].Severity);
    }

    [Fact]
    public void Evaluate_WhenAgeMatches_ReturnsEmpty()
    {
        var character = new WorldCharacter
        {
            Id = Guid.NewGuid(),
            FullName = "Aldric",
            BirthDate = new DateOnly(1995, 1, 1),
            Age = 29
        };
        var timelineEvent = new TimelineEvent
        {
            Id = Guid.NewGuid(),
            Title = "Battle",
            EventDate = new DateOnly(2024, 6, 1),
            RelatedEntityIds = [character.Id]
        };

        var results = AgeTimelineContradictionRule.EvaluatePair(
            character, timelineEvent, Guid.NewGuid(), Guid.NewGuid()).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void Rule_IsRegisteredForCreativeTenant()
    {
        Assert.Contains(TenantIds.Creative, _rule.SupportedTenantIds);
        Assert.Equal(ForgeRules.AgeTimeline, _rule.RuleId);
    }
}
