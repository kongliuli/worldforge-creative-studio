using Microsoft.Extensions.AI;
using WorldForge.AI.Plugins;
using WorldForge.Core.Abstractions;
using WorldForge.Core.Entities;
using WorldForge.Core.Tenants;
using WorldForge.Core.Tenants.Creative.Entities;

namespace WorldForge.AI.Tests;

public class WorldBuildingPluginTests
{
    [Fact]
    public async Task DetectAsync_EmptyNotes_ReturnsEmpty()
    {
        var plugin = new WorldBuildingPlugin(new NullChatClient());
        var result = await plugin.DetectAsync(new ContradictionDetectionRequest(
            Guid.NewGuid(), null, [], []));

        Assert.Empty(result);
    }

    [Fact]
    public void Plugin_IsBoundToCreativeTenant()
    {
        var plugin = new WorldBuildingPlugin(new NullChatClient());
        Assert.Equal(ForgePlugins.WorldBuilding, plugin.PluginId);
        Assert.Contains(TenantIds.Creative, plugin.SupportedTenantIds);
    }

    [Fact]
    public void ParseFindings_MapsJsonToFindings()
    {
        const string json = """
            {"contradictions":[{"type":"AgeMismatch","severity":"Warning","summary":"年龄不符","relatedEntityId":"11111111-1111-1111-1111-111111111111"}]}
            """;

        var findings = WorldBuildingPlugin.ParseFindings(json);
        Assert.Single(findings);
        Assert.Equal("AgeMismatch", findings[0].Type);
        Assert.Equal("年龄不符", findings[0].Summary);
    }

    [Fact]
    public async Task DetectAsync_UsesPreferredOllamaModelFromRequest()
    {
        var capturing = new CapturingChatClient();
        var plugin = new WorldBuildingPlugin(capturing);
        var notes = new List<Note>
        {
            new WorldCharacter { Id = Guid.NewGuid(), Title = "A", FullName = "A", Age = 20 },
        };

        await plugin.DetectAsync(new ContradictionDetectionRequest(
            Guid.NewGuid(), null, notes, [], "llama3.2"));

        Assert.Equal("llama3.2", capturing.LastModelId);
    }

    [Fact]
    public async Task DetectAsync_ParsesChatClientJson()
    {
        const string llmJson = """
            {"contradictions":[{"type":"TimelineOrder","severity":"Error","summary":"时间线冲突"}]}
            """;
        var plugin = new WorldBuildingPlugin(new StubChatClient(llmJson));
        var notes = new List<Note>
        {
            new WorldCharacter { Id = Guid.NewGuid(), Title = "A", FullName = "A", Age = 20 },
        };

        var result = await plugin.DetectAsync(new ContradictionDetectionRequest(
            Guid.NewGuid(), null, notes, []));

        Assert.Single(result);
        Assert.Equal("时间线冲突", result[0].Summary);
    }

    private sealed class CapturingChatClient : IChatClient
    {
        public string? LastModelId { get; private set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            LastModelId = options?.ModelId;
            return Task.FromResult(new ChatResponse([]));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            AsyncEnumerable.Empty<ChatResponseUpdate>();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private sealed class NullChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ChatResponse([]));

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            AsyncEnumerable.Empty<ChatResponseUpdate>();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private sealed class StubChatClient(string text) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            AsyncEnumerable.Empty<ChatResponseUpdate>();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
