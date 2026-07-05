using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Creative.Entities;

public class WorldFaction : Note, IEntityNode
{
    public string Name { get; set; } = "";
    public string CustomFieldsJson { get; set; } = "{}";

    public string DisplayName => Name;
}
