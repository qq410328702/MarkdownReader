using FluentAssertions;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests;

public class SearchServiceTests
{
    private readonly SearchService _sut = new();

    [Fact]
    public void Search_NullContent_ReturnsZeroMatches()
    {
        var result = _sut.Search(null!, "test");
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
    }

    [Fact]
    public void Search_EmptyContent_ReturnsZeroMatches()
    {
        var result = _sut.Search("", "test");
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
    }

    [Fact]
    public void Search_NullKeyword_ReturnsZeroMatches()
    {
        var result = _sut.Search("some content", null!);
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
    }

    [Fact]
    public void Search_EmptyKeyword_ReturnsZeroMatches()
    {
        var result = _sut.Search("some content", "");
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
    }

    [Fact]
    public void Search_NoMatch_ReturnsZeroMatches()
    {
        var result = _sut.Search("hello world", "xyz");
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
        result.Keyword.Should().Be("xyz");
    }

    [Fact]
    public void Search_SingleMatch_ReturnsCorrectPosition()
    {
        var result = _sut.Search("hello world", "world");
        result.TotalMatches.Should().Be(1);
        result.MatchPositions.Should().Equal(6);
    }

    [Fact]
    public void Search_MultipleMatches_ReturnsAllPositions()
    {
        var result = _sut.Search("cat and cat and cat", "cat");
        result.TotalMatches.Should().Be(3);
        result.MatchPositions.Should().Equal(0, 8, 16);
    }

    [Fact]
    public void Search_CaseInsensitive_FindsAllVariants()
    {
        var result = _sut.Search("Hello HELLO hello hElLo", "hello");
        result.TotalMatches.Should().Be(4);
        result.MatchPositions.Should().Equal(0, 6, 12, 18);
    }

    [Fact]
    public void Search_OverlappingMatches_FindsAll()
    {
        var result = _sut.Search("aaa", "aa");
        result.TotalMatches.Should().Be(2);
        result.MatchPositions.Should().Equal(0, 1);
    }

    [Fact]
    public void Search_KeywordSetsCorrectly()
    {
        var result = _sut.Search("test", "test");
        result.Keyword.Should().Be("test");
    }

    [Fact]
    public void Search_KeywordLongerThanContent_ReturnsZeroMatches()
    {
        var result = _sut.Search("hi", "hello");
        result.TotalMatches.Should().Be(0);
        result.MatchPositions.Should().BeEmpty();
    }
}
