using WorldForge.Core.Entities;

namespace WorldForge.Core.Services;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> ListRecentAsync(CancellationToken ct = default);
    Task<Project?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Project> CreateAsync(string name, CancellationToken ct = default);
    Task<bool> UpdateSettingsAsync(
        Guid id,
        bool? autoDetectOnSave = null,
        string? preferredOllamaModel = null,
        CancellationToken ct = default);
}

public interface IContradictionService
{
    Task<IReadOnlyList<Contradiction>> DetectAsync(
        Guid projectId,
        Guid? focusDocumentId = null,
        CancellationToken ct = default);
}
