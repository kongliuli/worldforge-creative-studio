namespace WorldForge.Core.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string TenantTemplateId { get; set; } = "creative";
    public string DbFilePath { get; set; } = "";
    public DateTime LastOpenedAt { get; set; }
    public ProjectSettings Settings { get; set; } = new();

    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Contradiction> Contradictions { get; set; } = [];
}

public class ProjectSettings
{
    public string DefaultEra { get; set; } = "";
    public bool AutoDetectOnSave { get; set; }
    public string PreferredOllamaModel { get; set; } = "phi4-mini";
}
