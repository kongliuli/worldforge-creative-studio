using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using WorldForge.AI.Clients;
using WorldForge.AI.Plugins;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Tenants;

namespace WorldForge.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddWorldForgeAi(
        this IServiceCollection services,
        Uri? ollamaBaseUrl = null)
    {
        var baseUrl = ollamaBaseUrl ?? new Uri("http://localhost:11434");
        services.AddSingleton<IChatClient>(_ => new OllamaChatClient(baseUrl));
        services.AddSingleton<WorldBuildingPlugin>();
        services.AddSingleton<IContradictionDetectionPlugin>(sp => sp.GetRequiredService<WorldBuildingPlugin>());
        services.AddSingleton<IWorldForgePlugin>(sp => sp.GetRequiredService<WorldBuildingPlugin>());
        return services;
    }
}
