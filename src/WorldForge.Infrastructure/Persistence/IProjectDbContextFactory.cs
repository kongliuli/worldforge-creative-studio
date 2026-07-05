using WorldForge.Core.Entities;

namespace WorldForge.Infrastructure.Persistence;

public interface IProjectDbContextFactory
{
    WorldForgeDbContext CreateForProject(Project project);

    Task EnsureProjectDatabaseAsync(Project project, CancellationToken ct = default);
}
