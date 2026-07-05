using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorldForge.Core.Entities;
using WorldForge.Core.Services;
using WorldForge.Core.Tenants.Creative.Entities;
using WorldForge.Infrastructure.Persistence;

namespace WorldForge.Infrastructure.Services;

public class ManuscriptService(
    IProjectService projectService,
    IProjectDbContextFactory dbFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<ManuscriptService>? logger = null) : IManuscriptService
{
    public async Task<IReadOnlyList<ManuscriptTreeNode>> GetTreeAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return [];

        await using var db = dbFactory.CreateForProject(project);
        return await db.Set<ManuscriptDocument>()
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.SortOrder)
            .Select(m => new ManuscriptTreeNode(
                m.Id, m.Title, m.ParentId, m.SortOrder, m.WordCount, m.Status))
            .ToListAsync(ct);
    }

    public async Task<ManuscriptDocument?> GetDocumentAsync(
        Guid projectId, Guid documentId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return null;

        await using var db = dbFactory.CreateForProject(project);
        return await db.Set<ManuscriptDocument>()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.Id == documentId, ct);
    }

    public async Task<IReadOnlyList<EntityLink>> GetLinksAsync(
        Guid projectId, Guid documentId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return [];

        await using var db = dbFactory.CreateForProject(project);
        var exists = await db.Set<ManuscriptDocument>()
            .AnyAsync(m => m.ProjectId == projectId && m.Id == documentId, ct);
        if (!exists) return [];

        return await db.EntityLinks
            .Where(l => l.SourceDocumentId == documentId)
            .ToListAsync(ct);
    }

    public async Task<ManuscriptDocument?> CreateAsync(
        Guid projectId,
        string title,
        Guid? parentId,
        int? sortOrder = null,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return null;

        await using var db = dbFactory.CreateForProject(project);

        if (parentId is not null &&
            !await db.Set<ManuscriptDocument>()
                .AnyAsync(m => m.ProjectId == projectId && m.Id == parentId, ct))
            return null;

        var order = sortOrder ?? await NextSortOrderAsync(db, projectId, parentId, ct);
        var now = DateTime.UtcNow;
        var doc = new ManuscriptDocument
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title,
            ParentId = parentId,
            SortOrder = order,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Set<ManuscriptDocument>().Add(doc);
        await db.SaveChangesAsync(ct);
        return doc;
    }

    public async Task<bool> SaveAsync(
        Guid projectId,
        Guid documentId,
        SaveManuscriptRequest request,
        CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        await using var db = dbFactory.CreateForProject(project);
        var doc = await db.Set<ManuscriptDocument>()
            .Include(m => m.EntityLinks)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.Id == documentId, ct);
        if (doc is null) return false;

        if (request.Title is not null)
            doc.Title = request.Title;
        if (request.ContentJson is not null)
        {
            doc.ContentJson = request.ContentJson;
            doc.WordCount = TipTapWordCounter.CountWords(request.ContentJson);
        }
        if (request.Status is not null)
            doc.Status = request.Status.Value;

        if (request.EntityLinks is not null)
        {
            db.EntityLinks.RemoveRange(doc.EntityLinks);
            doc.EntityLinks = request.EntityLinks.Select(l => new EntityLink
            {
                Id = Guid.NewGuid(),
                SourceDocumentId = documentId,
                TargetEntityId = l.TargetEntityId,
                StartOffset = l.StartOffset,
                EndOffset = l.EndOffset
            }).ToList();
        }

        doc.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        if (project.Settings.AutoDetectOnSave)
            ScheduleAutoDetect(projectId, documentId);

        return true;
    }

    private void ScheduleAutoDetect(Guid projectId, Guid documentId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var detector = scope.ServiceProvider.GetRequiredService<IContradictionService>();
                await detector.DetectAsync(projectId, documentId);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "AutoDetectOnSave failed for project {ProjectId}", projectId);
            }
        });
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken ct = default)
    {
        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        await using var db = dbFactory.CreateForProject(project);
        var all = await db.Set<ManuscriptDocument>()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(ct);
        if (all.All(m => m.Id != documentId))
            return false;

        var toDelete = CollectSubtree(all, documentId);
        var ids = toDelete.Select(m => m.Id).ToHashSet();
        var links = await db.EntityLinks
            .Where(l => ids.Contains(l.SourceDocumentId))
            .ToListAsync(ct);

        db.EntityLinks.RemoveRange(links);
        db.Set<ManuscriptDocument>().RemoveRange(toDelete);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ReorderAsync(
        Guid projectId,
        IReadOnlyList<(Guid Id, Guid? ParentId, int SortOrder)> items,
        CancellationToken ct = default)
    {
        if (items.Count == 0) return false;

        var project = await projectService.GetAsync(projectId, ct);
        if (project is null) return false;

        await using var db = dbFactory.CreateForProject(project);
        var docs = await db.Set<ManuscriptDocument>()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(ct);
        var byId = docs.ToDictionary(m => m.Id);

        foreach (var (id, parentId, sortOrder) in items)
        {
            if (!byId.TryGetValue(id, out var doc))
                return false;
            if (parentId is not null && !byId.ContainsKey(parentId.Value))
                return false;
            doc.ParentId = parentId;
            doc.SortOrder = sortOrder;
            doc.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    private static async Task<int> NextSortOrderAsync(
        WorldForgeDbContext db, Guid projectId, Guid? parentId, CancellationToken ct)
    {
        var max = await db.Set<ManuscriptDocument>()
            .Where(m => m.ProjectId == projectId && m.ParentId == parentId)
            .Select(m => (int?)m.SortOrder)
            .MaxAsync(ct);
        return (max ?? -1) + 1;
    }

    private static List<ManuscriptDocument> CollectSubtree(
        IReadOnlyList<ManuscriptDocument> all, Guid rootId)
    {
        var childrenByParent = all
            .Where(m => m.ParentId is not null)
            .GroupBy(m => m.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<ManuscriptDocument>();
        var root = all.First(m => m.Id == rootId);
        var stack = new Stack<ManuscriptDocument>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            result.Add(current);
            if (childrenByParent.TryGetValue(current.Id, out var children))
            {
                foreach (var child in children)
                    stack.Push(child);
            }
        }

        return result;
    }
}
