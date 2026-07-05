using WorldForge.AI;
using WorldForge.Core;
using WorldForge.Infrastructure;
using WorldForge.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddWorldForgeCore();

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
var projectsRoot = Path.Combine(dataDir, "Projects");
Directory.CreateDirectory(projectsRoot);
var registryPath = Path.Combine(dataDir, "_registry.worldforge.db");
builder.Services.AddWorldForgeInfrastructure(
    $"Data Source={registryPath}",
    projectsRoot);
builder.Services.AddWorldForgeAi();

var app = builder.Build();

await WorldForge.Infrastructure.DependencyInjection.MigrateDatabaseAsync(app.Services);

if (app.Environment.IsDevelopment())
    app.UseCors();

app.MapHealthEndpoints();
app.MapTenantEndpoints();
app.MapProjectEndpoints();
app.MapManuscriptEndpoints();
app.MapEntityEndpoints();
app.MapImportEndpoints();
app.MapContradictionEndpoints();
app.MapLicenseEndpoints();

app.Run();
