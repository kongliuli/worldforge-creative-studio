using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Services;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;
using WorldForge.Infrastructure.Persistence;

namespace WorldForge.Infrastructure.Services;

public class EntityService(
    IProjectService projectService,
    IProjectDbContextFactory dbFactory,
    ITenantRegistry tenantRegistry) : IEntityService
{
    public async Task<IReadOnlyList<Note>> ListAsync(
        Guid projectId,
        string? discriminator = null,
        string? prefix = null,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return [];

        var allowed = GetAllowedTypes(project);
        if (allowed.Count == 0) return [];

        await using var db = dbFactory.CreateForProject(project);
        var query = db.Notes.Where(n => n.ProjectId == projectId);
        if (!string.IsNullOrEmpty(discriminator))
        {
            if (!allowed.ContainsKey(discriminator))
                return [];
            query = query.Where(n => EF.Property<string>(n, "Discriminator") == discriminator);
        }
        else
        {
            var names = allowed.Keys.ToList();
            query = query.Where(n => names.Contains(EF.Property<string>(n, "Discriminator")));
        }

        var notes = await query.OrderBy(n => n.Title).ToListAsync(ct);
        if (string.IsNullOrWhiteSpace(prefix))
            return notes;

        var p = prefix.Trim();
        return notes
            .Where(n => GetDisplayName(n).Contains(p, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<Note?> GetAsync(Guid projectId, Guid entityId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return null;

        await using var db = dbFactory.CreateForProject(project);
        return await db.Notes.FirstOrDefaultAsync(n => n.ProjectId == projectId && n.Id == entityId, ct);
    }

    public async Task<Note?> CreateAsync(
        Guid projectId, CreateEntityRequest request, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return null;

        var allowed = GetAllowedTypes(project);
        if (!allowed.TryGetValue(request.Discriminator, out var clrType))
            return null;

        if (clrType == typeof(ManuscriptDocument))
            return null;

        await using var db = dbFactory.CreateForProject(project);
        var entity = (Note)Activator.CreateInstance(clrType)!;
        var now = DateTime.UtcNow;
        entity.Id = Guid.NewGuid();
        entity.ProjectId = projectId;
        entity.Title = request.Title.Trim();
        entity.ContentJson = request.ContentJson ?? "{}";
        entity.CreatedAt = now;
        entity.UpdatedAt = now;
        ApplyDisplayName(entity, entity.Title);

        db.Notes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> UpdateAsync(
        Guid projectId, Guid entityId, CreateEntityRequest request, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        var allowed = GetAllowedTypes(project);
        if (!allowed.ContainsKey(request.Discriminator))
            return false;

        await using var db = dbFactory.CreateForProject(project);
        var entity = await db.Notes.FirstOrDefaultAsync(n => n.ProjectId == projectId && n.Id == entityId, ct);
        if (entity is null) return false;

        entity.Title = request.Title.Trim();
        if (request.ContentJson is not null)
            entity.ContentJson = request.ContentJson;
        ApplyDisplayName(entity, entity.Title);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid entityId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        await using var db = dbFactory.CreateForProject(project);
        var entity = await db.Notes.FirstOrDefaultAsync(n => n.ProjectId == projectId && n.Id == entityId, ct);
        if (entity is null) return false;

        db.Notes.Remove(entity);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private IReadOnlyDictionary<string, Type> GetAllowedTypes(Project project)
    {
        var tenant = tenantRegistry.Get(project.TenantTemplateId);
        return tenant.EntityTypes
            .Where(t => t != typeof(ManuscriptDocument))
            .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
    }

    private static void ApplyDisplayName(Note entity, string title)
    {
        switch (entity)
        {
            case WorldCharacter wc:
                wc.FullName = title;
                break;
            case WorldLocation wl:
                wl.Name = title;
                break;
            case WorldFaction wf:
                wf.Name = title;
                break;
            case WorldItem wi:
                wi.Name = title;
                break;
        }
    }

    private static string GetDisplayName(Note note) =>
        note is IEntityNode node ? node.DisplayName : note.Title;
}
