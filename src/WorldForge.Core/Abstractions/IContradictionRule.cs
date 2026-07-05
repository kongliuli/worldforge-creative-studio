using WorldForge.Core.Entities;

namespace WorldForge.Core.Abstractions;

public record ContradictionRuleContext(
    Guid ProjectId,
    Guid DetectionRunId,
    IReadOnlyList<Note> Notes,
    IReadOnlyList<EntityLink> Links);

/// <summary>
/// 确定性矛盾规则 — 不依赖 LLM，各租户注册不同规则集。
/// </summary>
public interface IContradictionRule
{
    string RuleId { get; }
    IReadOnlyList<string> SupportedTenantIds { get; }
    IEnumerable<Contradiction> Evaluate(ContradictionRuleContext context);
}
