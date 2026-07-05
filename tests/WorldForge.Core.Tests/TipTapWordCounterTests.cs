using WorldForge.Core.Services;

namespace WorldForge.Core.Tests;

public class TipTapWordCounterTests
{
    [Fact]
    public void CountWords_ExtractsTextFromTipTapJson()
    {
        const string json = """
            {"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"Hello world"}]}]}
            """;

        Assert.Equal(2, TipTapWordCounter.CountWords(json));
    }

    [Fact]
    public void CountWords_EmptyOrInvalid_ReturnsZero()
    {
        Assert.Equal(0, TipTapWordCounter.CountWords(""));
        Assert.Equal(0, TipTapWordCounter.CountWords("{not json"));
    }
}
