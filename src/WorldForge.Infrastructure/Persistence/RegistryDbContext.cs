using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Entities;

namespace WorldForge.Infrastructure.Persistence;

/// <summary>项目元数据注册库 — 仅 Projects 表（04 §九）。</summary>
public class RegistryDbContext(DbContextOptions<RegistryDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.OwnsOne(p => p.Settings);
        });
    }
}
