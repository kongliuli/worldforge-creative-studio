using WorldForge.Core.Entities;
using WorldForge.Core.Enums;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Core.Services;

public record ManuscriptTreeNode(
    Guid Id,
    string Title,
    Guid? ParentId,
    int SortOrder,
    int WordCount,
    DocumentStatus Status);

public record EntityLinkInput(Guid TargetEntityId, int StartOffset, int EndOffset);

public record SaveManuscriptRequest(
    string? Title,
    string? ContentJson,
    DocumentStatus? Status,
    IReadOnlyList<EntityLinkInput>? EntityLinks);

public interface IManuscriptService
{
    Task<IReadOnlyList<ManuscriptTreeNode>> GetTreeAsync(Guid projectId, CancellationToken ct = default);
    Task<ManuscriptDocument?> GetDocumentAsync(Guid projectId, Guid documentId, CancellationToken ct = default);
    Task<IReadOnlyList<EntityLink>> GetLinksAsync(Guid projectId, Guid documentId, CancellationToken ct = default);
    Task<ManuscriptDocument?> CreateAsync(
        Guid projectId, string title, Guid? parentId, int? sortOrder = null, CancellationToken ct = default);
    Task<bool> SaveAsync(Guid projectId, Guid documentId, SaveManuscriptRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid projectId, Guid documentId, CancellationToken ct = default);
    Task<bool> ReorderAsync(
        Guid projectId,
        IReadOnlyList<(Guid Id, Guid? ParentId, int SortOrder)> items,
        CancellationToken ct = default);
}
