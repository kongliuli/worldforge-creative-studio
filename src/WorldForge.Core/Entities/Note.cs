namespace WorldForge.Core.Entities;

public abstract class Note
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string ContentJson { get; set; } = "{}";
    public float[]? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<EntityTag> Tags { get; set; } = [];
}
