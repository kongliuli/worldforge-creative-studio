using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Enums;
using WorldForge.Core.Services;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;
using WorldForge.Infrastructure.Persistence;

namespace WorldForge.Infrastructure.Services;

/// <summary>
/// 租户无关的矛盾检测编排 — 按项目 TenantTemplateId 加载规则 + AI 插件。
/// </summary>
public class ContradictionService(
    IProjectService projectService,
    IProjectDbContextFactory dbFactory,
    ITenantRegistry tenantRegistry,
    IEnumerable<IContradictionRule> rules,
    IEnumerable<IContradictionDetectionPlugin> plugins) : IContradictionService
{
    public async Task<IReadOnlyList<Contradiction>> DetectAsync(
        Guid projectId,
        Guid? focusDocumentId = null,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null)
            return [];

        var tenant = tenantRegistry.GetRequired(project.TenantTemplateId);
        if (!tenant.Features.ContradictionDetection)
            return [];

        await using var db = dbFactory.CreateForProject(project);

        var runId = Guid.NewGuid();
        var notes = await db.Notes.Where(n => n.ProjectId == projectId).ToListAsync(ct);
        var manuscriptIds = notes.OfType<ManuscriptDocument>().Select(d => d.Id).ToHashSet();
        var links = await db.EntityLinks
            .Where(l => manuscriptIds.Contains(l.SourceDocumentId))
            .ToListAsync(ct);

        var ruleContext = new ContradictionRuleContext(projectId, runId, notes, links);
        var found = new List<Contradiction>();

        foreach (var ruleId in tenant.ContradictionRuleIds)
        {
            var rule = rules.FirstOrDefault(r =>
                r.RuleId == ruleId &&
                r.SupportedTenantIds.Contains(tenant.Id, StringComparer.OrdinalIgnoreCase));
            if (rule is not null)
                found.AddRange(rule.Evaluate(ruleContext));
        }

        var detectionRequest = new ContradictionDetectionRequest(
            projectId, focusDocumentId, notes, links, project.Settings.PreferredOllamaModel);
        foreach (var pluginId in tenant.PluginIds)
        {
            var plugin = plugins.FirstOrDefault(p =>
                p.PluginId == pluginId &&
                p.SupportedTenantIds.Contains(tenant.Id, StringComparer.OrdinalIgnoreCase));
            if (plugin is null) continue;

            var findings = await plugin.DetectAsync(detectionRequest, ct);
            found.AddRange(findings.Select(f => MapFinding(f, projectId, runId)));
        }

        if (found.Count > 0)
        {
            db.Contradictions.AddRange(found);
            await db.SaveChangesAsync(ct);
        }

        return found;
    }

    private static Contradiction MapFinding(ContradictionFinding f, Guid projectId, Guid runId) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Type = Enum.TryParse<ContradictionType>(f.Type, out var type) ? type : ContradictionType.AttributeConflict,
            Severity = Enum.TryParse<ContradictionSeverity>(f.Severity, out var sev) ? sev : ContradictionSeverity.Warning,
            Summary = f.Summary,
            SourceDocumentId = f.SourceDocumentId,
            TargetDocumentId = f.TargetDocumentId,
            SourceExcerpt = f.SourceExcerpt,
            TargetExcerpt = f.TargetExcerpt,
            RelatedEntityId = f.RelatedEntityId,
            Suggestion = f.Suggestion,
            Status = ContradictionStatus.Open,
            DetectedAt = DateTime.UtcNow,
            DetectionRunId = runId
        };
}
