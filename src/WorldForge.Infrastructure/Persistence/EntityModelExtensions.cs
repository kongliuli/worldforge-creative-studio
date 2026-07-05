using Microsoft.EntityFrameworkCore;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;
using WorldForge.Core.Tenants.Health.Entities;
using WorldForge.Core.Tenants.Legacy.Entities;

namespace WorldForge.Infrastructure.Persistence;

internal static class EntityModelExtensions
{
    public static void ApplyNoteHierarchy(this ModelBuilder modelBuilder)
    {
        EntityTypeCatalog.EnsureInitialized();

        var note = modelBuilder.Entity<Core.Entities.Note>();
        note.HasKey(n => n.Id);

        var discriminator = note.HasDiscriminator<string>("Discriminator");
        foreach (var (name, clrType) in EntityTypeCatalog.All)
            discriminator.HasValue(clrType, name);

        note.Property(n => n.Embedding).HasConversion(WorldForgeDbContext.EmbeddingConverter);
    }

    public static void ApplyTenantSpecificProperties(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorldCharacter>(e =>
            e.Property(c => c.Aliases).HasConversion(WorldForgeDbContext.StringListConverter));

        modelBuilder.Entity<TimelineEvent>(e =>
            e.Property(t => t.RelatedEntityIds).HasConversion(WorldForgeDbContext.GuidListConverter));

        modelBuilder.Entity<LifeEvent>(e =>
            e.Property(t => t.RelatedEntityIds).HasConversion(WorldForgeDbContext.GuidListConverter));

        modelBuilder.Entity<DoctorVisit>(e =>
            e.Property(t => t.RelatedEntityIds).HasConversion(WorldForgeDbContext.GuidListConverter));
    }
}
