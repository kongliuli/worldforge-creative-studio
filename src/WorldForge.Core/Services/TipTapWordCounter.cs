using System.Text;
using System.Text.Json;

namespace WorldForge.Core.Services;

public static class TipTapWordCounter
{
    public static int CountWords(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            return 0;

        try
        {
            using var doc = JsonDocument.Parse(contentJson);
            var text = new StringBuilder();
            CollectText(doc.RootElement, text);
            return text.Length == 0
                ? 0
                : text.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    private static void CollectText(JsonElement element, StringBuilder text)
    {
        if (element.TryGetProperty("text", out var textNode))
            text.Append(textNode.GetString()).Append(' ');

        if (element.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in content.EnumerateArray())
                CollectText(child, text);
        }
    }
}
