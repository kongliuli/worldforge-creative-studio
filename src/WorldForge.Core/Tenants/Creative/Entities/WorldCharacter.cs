using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Creative.Entities;

public class WorldCharacter : Note, IAgeAwareEntity
{
    public string FullName { get; set; } = "";
    public List<string> Aliases { get; set; } = [];
    public int? Age { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Guid? FactionId { get; set; }
    public string? Race { get; set; }
    public string? Gender { get; set; }
    public string? Alignment { get; set; }
    public string CustomFieldsJson { get; set; } = "{}";

    public string DisplayName => FullName;
}
