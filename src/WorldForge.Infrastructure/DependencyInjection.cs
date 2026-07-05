using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldForge.Core.Services;
using WorldForge.Infrastructure.Persistence;
using WorldForge.Infrastructure.Services;

namespace WorldForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWorldForgeInfrastructure(
        this IServiceCollection services,
        string registryConnectionString,
        string projectsRoot)
    {
        services.AddSingleton(new ProjectDatabaseOptions
        {
            RegistryConnectionString = registryConnectionString,
            ProjectsRoot = projectsRoot
        });
        services.AddSingleton<IProjectDbContextFactory, ProjectDbContextFactory>();

        services.AddDbContext<RegistryDbContext>(options =>
            options.UseSqlite(registryConnectionString));

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IContradictionService, ContradictionService>();
        services.AddScoped<IManuscriptService, ManuscriptService>();
        services.AddScoped<IEntityService, EntityService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IContradictionQueryService, ContradictionQueryService>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var registry = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
        await registry.Database.EnsureCreatedAsync(ct);
    }
}
