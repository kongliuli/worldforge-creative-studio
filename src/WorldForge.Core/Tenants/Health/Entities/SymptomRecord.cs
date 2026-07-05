using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Health.Entities;

/// <summary>映射文档：WorldCharacter → SymptomRecord</summary>
public class SymptomRecord : Note, IEntityNode
{
    public string SymptomName { get; set; } = "";
    public string Severity { get; set; } = "";

    public string DisplayName => SymptomName;
}
