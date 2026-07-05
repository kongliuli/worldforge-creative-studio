using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Entities;
using WorldForge.Core.Services;
using WorldForge.Core.Tenants;
using WorldForge.Infrastructure.Persistence;

namespace WorldForge.Infrastructure.Services;

public class ProjectService(
    RegistryDbContext db,
    ITenantContext tenantContext,
    IProjectDbContextFactory dbFactory) : IProjectService
{
    public async Task<IReadOnlyList<Project>> ListRecentAsync(CancellationToken ct = default) =>
        await db.Projects
            .OrderByDescending(p => p.LastOpenedAt)
            .Take(20)
            .ToListAsync(ct);

    public Task<Project?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Project> CreateAsync(string name, CancellationToken ct = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            TenantTemplateId = tenantContext.Current.Id,
            DbFilePath = "",
            LastOpenedAt = DateTime.UtcNow
        };
        project.DbFilePath = $"{project.Id:N}.worldforge.db";

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        await dbFactory.EnsureProjectDatabaseAsync(project, ct);
        return project;
    }

    public async Task<bool> UpdateSettingsAsync(
        Guid id,
        bool? autoDetectOnSave = null,
        string? preferredOllamaModel = null,
        CancellationToken ct = default)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return false;

        if (autoDetectOnSave is not null)
            project.Settings.AutoDetectOnSave = autoDetectOnSave.Value;
        if (preferredOllamaModel is not null)
            project.Settings.PreferredOllamaModel = preferredOllamaModel.Trim();

        await db.SaveChangesAsync(ct);
        return true;
    }
}
