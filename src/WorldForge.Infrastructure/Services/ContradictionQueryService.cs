using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Entities;
using WorldForge.Core.Enums;
using WorldForge.Core.Services;
using WorldForge.Infrastructure.Persistence;

namespace WorldForge.Infrastructure.Services;

public class ContradictionQueryService(
    IProjectService projectService,
    IProjectDbContextFactory dbFactory) : IContradictionQueryService
{
    public async Task<IReadOnlyList<Contradiction>> ListAsync(
        Guid projectId,
        ContradictionStatus? status = null,
        ContradictionSeverity? severity = null,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return [];

        await using var db = dbFactory.CreateForProject(project);
        var query = db.Contradictions.Where(c => c.ProjectId == projectId);
        if (status is not null)
            query = query.Where(c => c.Status == status);
        if (severity is not null)
            query = query.Where(c => c.Severity == severity);

        return await query
            .OrderByDescending(c => c.DetectedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateStatusAsync(
        Guid projectId,
        Guid contradictionId,
        ContradictionStatus status,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        await using var db = dbFactory.CreateForProject(project);
        var item = await db.Contradictions
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Id == contradictionId, ct);
        if (item is null) return false;

        item.Status = status;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
