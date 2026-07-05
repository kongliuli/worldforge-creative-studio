using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Entities;

namespace WorldForge.Infrastructure.Persistence;

public class ProjectDbContextFactory(ProjectDatabaseOptions options) : IProjectDbContextFactory
{
    public WorldForgeDbContext CreateForProject(Project project)
    {
        var path = ResolvePath(project);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var dbOptions = new DbContextOptionsBuilder<WorldForgeDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new WorldForgeDbContext(dbOptions);
    }

    public async Task EnsureProjectDatabaseAsync(Project project, CancellationToken ct = default)
    {
        await using var db = CreateForProject(project);
        await db.Database.MigrateAsync(ct);
    }

    private string ResolvePath(Project project)
    {
        var fileName = string.IsNullOrWhiteSpace(project.DbFilePath)
            ? $"{project.Id:N}.worldforge.db"
            : project.DbFilePath;
        return Path.Combine(options.ProjectsRoot, fileName);
    }
}
