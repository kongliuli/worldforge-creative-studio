using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.AI.Plugins;

/// <summary>
/// Creative 租户 AI 插件 — 调用 Ollama 做 LLM 矛盾检测；不可用时降级为空。
/// </summary>
public sealed class WorldBuildingPlugin(IChatClient chatClient, ILogger<WorldBuildingPlugin>? logger = null)
    : IContradictionDetectionPlugin
{
    private static readonly string SystemPrompt = """
        你是世界观连续性编辑，只报告矛盾，不生成故事正文。
        根据实体卡片、时间线与章节摘录，找出设定冲突。
        输出必须是 JSON，符合 schema，不要 markdown 包裹。
        schema: {"contradictions":[{"type":"AgeMismatch|NameVariant|LocationConflict|TimelineOrder|AttributeConflict","severity":"Info|Warning|Error","summary":"string","sourceDocumentId":"uuid?","sourceExcerpt":"string?","targetDocumentId":"uuid?","targetExcerpt":"string?","relatedEntityId":"uuid?","suggestion":"string?"}]}
        """;

    public string PluginId => ForgePlugins.WorldBuilding;
    public IReadOnlyList<string> SupportedTenantIds { get; } = [TenantIds.Creative];

    public async Task<IReadOnlyList<ContradictionFinding>> DetectAsync(
        ContradictionDetectionRequest request,
        CancellationToken ct = default)
    {
        if (request.Notes.Count == 0)
            return [];

        try
        {
            var userPrompt = BuildUserPrompt(request);
            var response = await chatClient.GetResponseAsync(
                [
                    new ChatMessage(ChatRole.System, SystemPrompt),
                    new ChatMessage(ChatRole.User, userPrompt),
                ],
                new ChatOptions { ModelId = request.PreferredOllamaModel },
                ct);

            var text = response.Text;
            if (string.IsNullOrWhiteSpace(text))
                return [];

            return ParseFindings(text);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "WorldBuildingPlugin Ollama call failed; skipping LLM track");
            return [];
        }
    }

    internal static string BuildUserPrompt(ContradictionDetectionRequest request)
    {
        var characters = request.Notes.OfType<WorldCharacter>()
            .Select(c => new { c.Id, c.FullName, c.Age, c.Race, c.FactionId })
            .ToList();

        var timelines = request.Notes.OfType<TimelineEvent>()
            .Select(t => new { t.Id, t.Title, t.EventDate, t.RelatedEntityIds })
            .ToList();

        var manuscripts = request.Notes.OfType<ManuscriptDocument>().ToList();
        ManuscriptDocument? focus = null;
        if (request.FocusDocumentId is Guid focusId)
            focus = manuscripts.FirstOrDefault(m => m.Id == focusId);

        var excerpts = (focus is not null ? [focus] : manuscripts.Take(3))
            .Select(m => new { m.Id, m.Title, Content = Truncate(m.ContentJson, 2000) })
            .ToList();

        var payload = new { characters, timelines, excerpts, entityLinkCount = request.Links.Count };
        return JsonSerializer.Serialize(payload);
    }

    public static IReadOnlyList<ContradictionFinding> ParseFindings(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("contradictions", out var arr) ||
            arr.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<ContradictionFinding>();
        foreach (var item in arr.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString() ?? "AttributeConflict";
            var severity = item.GetProperty("severity").GetString() ?? "Warning";
            var summary = item.GetProperty("summary").GetString() ?? "";
            if (string.IsNullOrWhiteSpace(summary)) continue;

            results.Add(new ContradictionFinding(
                type,
                severity,
                summary,
                TryGuid(item, "sourceDocumentId"),
                item.TryGetProperty("sourceExcerpt", out var se) ? se.GetString() ?? "" : "",
                TryGuid(item, "targetDocumentId"),
                item.TryGetProperty("targetExcerpt", out var te) ? te.GetString() ?? "" : "",
                TryGuid(item, "relatedEntityId"),
                item.TryGetProperty("suggestion", out var sg) ? sg.GetString() : null));
        }

        return results;
    }

    private static Guid? TryGuid(JsonElement item, string name) =>
        item.TryGetProperty(name, out var el) &&
        el.ValueKind == JsonValueKind.String &&
        Guid.TryParse(el.GetString(), out var id)
            ? id
            : null;

    private static string Truncate(string contentJson, int max) =>
        contentJson.Length <= max ? contentJson : contentJson[..max];
}
