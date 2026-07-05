using WorldForge.Core.Entities;

namespace WorldForge.Core.Services;

public record CreateEntityRequest(
    string Discriminator,
    string Title,
    string? ContentJson = null);

public interface IEntityService
{
    Task<IReadOnlyList<Note>> ListAsync(
        Guid projectId, string? discriminator = null, string? prefix = null, CancellationToken ct = default);

    Task<Note?> GetAsync(Guid projectId, Guid entityId, CancellationToken ct = default);

    Task<Note?> CreateAsync(Guid projectId, CreateEntityRequest request, CancellationToken ct = default);

    Task<bool> UpdateAsync(Guid projectId, Guid entityId, CreateEntityRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid projectId, Guid entityId, CancellationToken ct = default);
}
