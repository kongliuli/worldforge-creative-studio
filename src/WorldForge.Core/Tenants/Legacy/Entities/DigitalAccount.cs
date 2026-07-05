using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Legacy.Entities;

/// <summary>映射文档：WorldCharacter → DigitalAccount</summary>
public class DigitalAccount : Note, IEntityNode
{
    public string AccountName { get; set; } = "";
    public string Provider { get; set; } = "";
    public string CustomFieldsJson { get; set; } = "{}";

    public string DisplayName => AccountName;
}
