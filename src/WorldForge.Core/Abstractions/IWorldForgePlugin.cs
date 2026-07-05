namespace WorldForge.Core.Abstractions;

/// <summary>
/// 引擎级插件标识 — Core 不引用 AI 实现，仅通过 PluginId 关联。
/// </summary>
public interface IWorldForgePlugin
{
    string PluginId { get; }
    IReadOnlyList<string> SupportedTenantIds { get; }
}
