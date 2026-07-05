using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WorldForge.Core.Entities;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.Infrastructure.Persistence;

public class WorldForgeDbContext(DbContextOptions<WorldForgeDbContext> options) : DbContext(options)
{
    internal static readonly ValueConverter<float[]?, string?> EmbeddingConverter = new(
        v => v == null ? null : JsonSerializer.Serialize(v),
        v => v == null ? null : JsonSerializer.Deserialize<float[]>(v));

    internal static readonly ValueConverter<List<string>, string> StringListConverter = new(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>());

    internal static readonly ValueConverter<List<Guid>, string> GuidListConverter = new(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<List<Guid>>(v) ?? new List<Guid>());

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Contradiction> Contradictions => Set<Contradiction>();
    public DbSet<EntityLink> EntityLinks => Set<EntityLink>();
    public DbSet<EntityRelationship> EntityRelationships => Set<EntityRelationship>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.OwnsOne(p => p.Settings);
            e.HasMany(p => p.Notes).WithOne(n => n.Project).HasForeignKey(n => n.ProjectId);
            e.HasMany(p => p.Contradictions).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId);
        });

        modelBuilder.ApplyNoteHierarchy();
        modelBuilder.ApplyTenantSpecificProperties();

        modelBuilder.Entity<ManuscriptDocument>(e =>
        {
            e.HasMany(d => d.EntityLinks)
                .WithOne(l => l.SourceDocument)
                .HasForeignKey(l => l.SourceDocumentId);
        });

        modelBuilder.Entity<EntityLink>(e =>
        {
            e.HasOne(l => l.TargetEntity)
                .WithMany()
                .HasForeignKey(l => l.TargetEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntityRelationship>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId);
        });

        modelBuilder.Entity<Note>().HasIndex(n => n.ProjectId);
        modelBuilder.Entity<Contradiction>().HasIndex(c => new { c.ProjectId, c.Status });
        modelBuilder.Entity<EntityLink>().HasIndex(l => l.SourceDocumentId);
    }
}
