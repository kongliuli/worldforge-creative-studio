using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorldForge.Infrastructure.Persistence;

/// <summary>dotnet ef 设计时工厂；连接串与 Server Program.cs 一致。</summary>
public class WorldForgeDbContextFactory : IDesignTimeDbContextFactory<WorldForgeDbContext>
{
    public WorldForgeDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "WorldForge.Server", "Data", "worldforge.dev.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var options = new DbContextOptionsBuilder<WorldForgeDbContext>()
            .UseSqlite($"Data Source={Path.GetFullPath(dbPath)}")
            .Options;

        return new WorldForgeDbContext(options);
    }
}
