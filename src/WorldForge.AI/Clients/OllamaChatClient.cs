using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace WorldForge.AI.Clients;

/// <summary>Ollama REST → IChatClient；不可用时由插件层降级为空结果。</summary>
public sealed class OllamaChatClient : IChatClient
{
    private readonly HttpClient _http;

    public OllamaChatClient(Uri baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = baseUrl,
            Timeout = TimeSpan.FromSeconds(2),
        };
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = chatMessages.Select(ToOllamaMessage).ToList();
        var body = new
        {
            model = options?.ModelId ?? "phi4-mini",
            messages,
            stream = false,
            format = "json",
        };

        using var resp = await _http.PostAsJsonAsync("/api/chat", body, cancellationToken);
        if (!resp.IsSuccessStatusCode)
            return new ChatResponse([]);

        var payload = await resp.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var text = payload?.Message?.Content ?? "";
        return string.IsNullOrWhiteSpace(text)
            ? new ChatResponse([])
            : new ChatResponse(new ChatMessage(ChatRole.Assistant, text));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default) =>
        AsyncEnumerable.Empty<ChatResponseUpdate>();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() => _http.Dispose();

    private static object ToOllamaMessage(ChatMessage message)
    {
        var role = message.Role.Value == ChatRole.System.Value ? "system"
            : message.Role.Value == ChatRole.Assistant.Value ? "assistant"
            : "user";
        return new { role, content = message.Text ?? "" };
    }

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
