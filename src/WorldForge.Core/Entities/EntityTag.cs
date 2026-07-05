namespace WorldForge.Core.Entities;

public class EntityTag
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public string Name { get; set; } = "";

    public Note Note { get; set; } = null!;
}
