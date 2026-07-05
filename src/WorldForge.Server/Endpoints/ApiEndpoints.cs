using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Enums;
using WorldForge.Core.Services;
using WorldForge.Core.Tenants;
using WorldForge.Shared.Dtos;

namespace WorldForge.Server.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tenants", (ITenantRegistry registry) =>
            Results.Ok(registry.All.Select(t => new TenantSummaryDto(
                t.Id,
                t.DisplayName,
                t.SecurityLevel.ToString(),
                t.SkinId,
                t.Features.ContradictionDetection))));

        app.MapGet("/api/tenants/{tenantId}", (string tenantId, ITenantRegistry registry) =>
        {
            var tenant = registry.Get(tenantId);
            return Results.Ok(new TenantDetailDto(
                tenant.Id,
                tenant.DisplayName,
                tenant.SecurityLevel.ToString(),
                tenant.SkinId,
                tenant.EntityTypes.Select(e => e.Name).ToList(),
                tenant.PluginIds.ToList(),
                new TenantFeatureFlagsDto(
                    tenant.Features.ContradictionDetection,
                    tenant.Features.Collaboration,
                    tenant.Features.CloudSync)));
        });

        return app;
    }
}

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new HealthResponse("ok", "0.1.0-skeleton")));
        return app;
    }
}

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        group.MapGet("/", async (IProjectService projects, CancellationToken ct) =>
        {
            var list = await projects.ListRecentAsync(ct);
            return Results.Ok(list.Select(p => new ProjectSummaryDto(p.Id, p.Name, p.LastOpenedAt, p.TenantTemplateId)));
        });

        group.MapPost("/", async (CreateProjectRequest req, IProjectService projects, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest("Name is required.");

            var project = await projects.CreateAsync(req.Name.Trim(), ct);
            return Results.Created($"/api/projects/{project.Id}",
                new ProjectSummaryDto(project.Id, project.Name, project.LastOpenedAt, project.TenantTemplateId));
        });

        group.MapGet("/{id:guid}", async (Guid id, IProjectService projects, CancellationToken ct) =>
        {
            var project = await projects.GetAsync(id, ct);
            return project is null
                ? Results.NotFound()
                : Results.Ok(new ProjectSummaryDto(project.Id, project.Name, project.LastOpenedAt, project.TenantTemplateId));
        });

        group.MapGet("/{id:guid}/settings", async (Guid id, IProjectService projects, CancellationToken ct) =>
        {
            var project = await projects.GetAsync(id, ct);
            return project is null
                ? Results.NotFound()
                : Results.Ok(new ProjectSettingsDto(
                    project.Settings.AutoDetectOnSave,
                    project.Settings.PreferredOllamaModel));
        });

        group.MapPatch("/{id:guid}/settings", async (
            Guid id,
            UpdateProjectSettingsRequest req,
            IProjectService projects,
            CancellationToken ct) =>
        {
            var ok = await projects.UpdateSettingsAsync(
                id, req.AutoDetectOnSave, req.PreferredOllamaModel, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}

public static class ContradictionEndpoints
{
    public static IEndpointRouteBuilder MapContradictionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/contradictions");

        group.MapGet("/", async (
            Guid projectId,
            string? status,
            string? severity,
            IContradictionQueryService query,
            CancellationToken ct) =>
        {
            ContradictionStatus? statusFilter = null;
            if (status is not null)
            {
                if (!Enum.TryParse<ContradictionStatus>(status, true, out var parsedStatus))
                    return Results.BadRequest("Invalid status.");
                statusFilter = parsedStatus;
            }

            ContradictionSeverity? severityFilter = null;
            if (severity is not null)
            {
                if (!Enum.TryParse<ContradictionSeverity>(severity, true, out var parsedSeverity))
                    return Results.BadRequest("Invalid severity.");
                severityFilter = parsedSeverity;
            }

            var items = await query.ListAsync(projectId, statusFilter, severityFilter, ct);
            return Results.Ok(items.Select(ToDto));
        });

        group.MapPost("/detect",
            async (Guid projectId, IContradictionService detector, CancellationToken ct) =>
            {
                var items = await detector.DetectAsync(projectId, ct: ct);
                return Results.Ok(items.Select(ToDto));
            });

        group.MapPatch("/{id:guid}", async (
            Guid projectId,
            Guid id,
            UpdateContradictionStatusRequest req,
            IContradictionQueryService query,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<ContradictionStatus>(req.Status, true, out var status))
                return Results.BadRequest("Invalid status.");

            var ok = await query.UpdateStatusAsync(projectId, id, status, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static ContradictionDto ToDto(Contradiction c) =>
        new(c.Id, c.Type.ToString(), c.Severity.ToString(), c.Summary, c.Status.ToString(),
            c.RelatedEntityId, c.Suggestion);
}

public static class LicenseEndpoints
{
    public static IEndpointRouteBuilder MapLicenseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/license/verify", (LicenseVerifyRequest req) =>
        {
            var valid = !string.IsNullOrWhiteSpace(req.LicenseKey) && req.LicenseKey.Length >= 8;
            return Results.Ok(new LicenseVerifyResponse(valid, valid ? null : "Invalid license key"));
        });

        return app;
    }
}

public static class ManuscriptEndpoints
{
    public static IEndpointRouteBuilder MapManuscriptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/manuscripts");

        group.MapGet("/", async (Guid projectId, IManuscriptService manuscripts, CancellationToken ct) =>
        {
            var tree = await manuscripts.GetTreeAsync(projectId, ct);
            return Results.Ok(tree.Select(ToNodeDto));
        });

        group.MapPost("/", async (
            Guid projectId,
            CreateManuscriptRequest req,
            IManuscriptService manuscripts,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest("Title is required.");

            var doc = await manuscripts.CreateAsync(
                projectId, req.Title.Trim(), req.ParentId, req.SortOrder, ct);
            return doc is null
                ? Results.NotFound()
                : Results.Created(
                    $"/api/projects/{projectId}/manuscripts/{doc.Id}",
                    ToNodeDto(new(
                        doc.Id, doc.Title, doc.ParentId, doc.SortOrder, doc.WordCount, doc.Status)));
        });

        group.MapGet("/{id:guid}", async (
            Guid projectId,
            Guid id,
            IManuscriptService manuscripts,
            CancellationToken ct) =>
        {
            var doc = await manuscripts.GetDocumentAsync(projectId, id, ct);
            if (doc is null)
                return Results.NotFound();

            var links = await manuscripts.GetLinksAsync(projectId, id, ct);
            return Results.Ok(new ManuscriptDetailDto(
                doc.Id,
                doc.Title,
                doc.ContentJson,
                doc.Status.ToString(),
                doc.WordCount,
                links.Select(l => new EntityLinkDto(l.TargetEntityId, l.StartOffset, l.EndOffset)).ToList()));
        });

        group.MapPut("/{id:guid}", async (
            Guid projectId,
            Guid id,
            UpdateManuscriptRequest req,
            IManuscriptService manuscripts,
            CancellationToken ct) =>
        {
            DocumentStatus? status = null;
            if (req.Status is not null)
            {
                if (!Enum.TryParse<DocumentStatus>(req.Status, true, out var parsed))
                    return Results.BadRequest("Invalid status.");
                status = parsed;
            }

            var links = req.EntityLinks?
                .Select(l => new EntityLinkInput(l.TargetEntityId, l.StartOffset, l.EndOffset))
                .ToList();

            var saved = await manuscripts.SaveAsync(
                projectId, id, new SaveManuscriptRequest(req.Title, req.ContentJson, status, links), ct);
            return saved ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (
            Guid projectId,
            Guid id,
            IManuscriptService manuscripts,
            CancellationToken ct) =>
        {
            var deleted = await manuscripts.DeleteAsync(projectId, id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/reorder", async (
            Guid projectId,
            ReorderManuscriptsRequest req,
            IManuscriptService manuscripts,
            CancellationToken ct) =>
        {
            if (req.Items is null || req.Items.Count == 0)
                return Results.BadRequest("Items are required.");

            var items = req.Items
                .Select(i => (i.Id, i.ParentId, i.SortOrder))
                .ToList();
            var ok = await manuscripts.ReorderAsync(projectId, items, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static ManuscriptNodeDto ToNodeDto(ManuscriptTreeNode node) =>
        new(node.Id, node.Title, node.ParentId, node.SortOrder, node.WordCount, node.Status.ToString());
}

public static class EntityEndpoints
{
    public static IEndpointRouteBuilder MapEntityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/entities");

        group.MapGet("/", async (
            Guid projectId,
            string? type,
            string? prefix,
            IEntityService entities,
            CancellationToken ct) =>
        {
            var list = await entities.ListAsync(projectId, type, prefix, ct);
            return Results.Ok(list.Select(ToSummaryDto));
        });

        group.MapPost("/", async (
            Guid projectId,
            UpsertEntityRequest req,
            IEntityService entities,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Discriminator) || string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest("Discriminator and Title are required.");

            var entity = await entities.CreateAsync(
                projectId, new CreateEntityRequest(req.Discriminator, req.Title, req.ContentJson), ct);
            return entity is null
                ? Results.BadRequest("Invalid discriminator or project.")
                : Results.Created(
                    $"/api/projects/{projectId}/entities/{entity.Id}",
                    ToSummaryDto(entity));
        });

        group.MapGet("/{id:guid}", async (
            Guid projectId,
            Guid id,
            IEntityService entities,
            CancellationToken ct) =>
        {
            var entity = await entities.GetAsync(projectId, id, ct);
            return entity is null
                ? Results.NotFound()
                : Results.Ok(new EntityDetailDto(
                    entity.Id,
                    entity.GetType().Name,
                    entity.Title,
                    entity.ContentJson));
        });

        group.MapPut("/{id:guid}", async (
            Guid projectId,
            Guid id,
            UpsertEntityRequest req,
            IEntityService entities,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Discriminator) || string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest("Discriminator and Title are required.");

            var ok = await entities.UpdateAsync(
                projectId, id, new CreateEntityRequest(req.Discriminator, req.Title, req.ContentJson), ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (
            Guid projectId,
            Guid id,
            IEntityService entities,
            CancellationToken ct) =>
        {
            var ok = await entities.DeleteAsync(projectId, id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static EntitySummaryDto ToSummaryDto(Note note) =>
        new(note.Id, note.GetType().Name, note is IEntityNode node ? node.DisplayName : note.Title);
}

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects/{projectId:guid}/import/zip",
            async (Guid projectId, IFormFile file, IImportService import, CancellationToken ct) =>
            {
                if (file.Length == 0 || file.Length > 50 * 1024 * 1024)
                    return Results.BadRequest("Zip file required (max 50MB).");

                await using var stream = file.OpenReadStream();
                var result = await import.ImportZipAsync(projectId, stream, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .DisableAntiforgery();

        return app;
    }
}
