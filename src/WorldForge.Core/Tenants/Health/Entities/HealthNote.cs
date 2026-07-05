using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;

namespace WorldForge.Core.Tenants.Health.Entities;

/// <summary>映射文档：ManuscriptDocument → HealthNote</summary>
public class HealthNote : Note, IEntityNode
{
    public string DisplayName => Title;
}
