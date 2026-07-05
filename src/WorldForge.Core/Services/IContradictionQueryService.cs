using WorldForge.Core.Entities;
using WorldForge.Core.Enums;

namespace WorldForge.Core.Services;

public interface IContradictionQueryService
{
    Task<IReadOnlyList<Contradiction>> ListAsync(
        Guid projectId,
        ContradictionStatus? status = null,
        ContradictionSeverity? severity = null,
        CancellationToken ct = default);

    Task<bool> UpdateStatusAsync(
        Guid projectId,
        Guid contradictionId,
        ContradictionStatus status,
        CancellationToken ct = default);
}
