using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Legacy.Entities;

/// <summary>映射文档：ManuscriptDocument → Letter / WillDraft</summary>
public class Letter : Note, IEntityNode
{
    public string Recipient { get; set; } = "";

    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Recipient : Title;
}
