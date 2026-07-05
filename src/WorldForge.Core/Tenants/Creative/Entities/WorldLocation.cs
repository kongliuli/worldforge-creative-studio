using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Enums;

namespace WorldForge.Core.Tenants.Creative.Entities;

public class WorldLocation : Note, IEntityNode
{
    public string Name { get; set; } = "";
    public LocationType LocationType { get; set; } = LocationType.Other;
    public Guid? ParentLocationId { get; set; }
    public string? CoordinatesJson { get; set; }
    public string CustomFieldsJson { get; set; } = "{}";

    public string DisplayName => Name;
}
