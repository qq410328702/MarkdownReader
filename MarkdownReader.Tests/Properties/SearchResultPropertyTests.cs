using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 7: 搜索结果完整性
/// For any 文本内容和搜索关键词，Search_Engine 返回的匹配数量应等于关键词在文本中实际出现的次数，
/// 且每个匹配位置应准确指向关键词在文本中的起始索引。
/// **Validates: Requirements 6.2, 6.3**
/// </summary>
public class SearchResultPropertyTests
{
    private readonly SearchService _sut = new();

    /// <summary>
    /// Generates a record containing text content and a keyword that appears in it.
    /// The keyword is a non-empty substring drawn from a pool of simple words,
    /// and the content is built by embedding the keyword among filler text segments.
    /// </summary>
    public static Arbitrary<(string Content, string Keyword)> ContentWithKeywordArbitrary()
    {
        var keywordGen = Gen.Elements(
            "hello", "world", "test", "search", "find",
            "abc", "xyz", "foo", "bar", "data");

        var fillerGen = Gen.Elements(
            " some text ", " between words ", " filler content ",
            " random stuff ", " more text here ", " padding ");

        var gen = keywordGen.SelectMany(keyword =>
            Gen.Choose(1, 5).SelectMany(occurrences =>
                Gen.ListOf(fillerGen, occurrences + 1)
                    .Select(fillers =>
                    {
                        // Interleave fillers with keyword occurrences
                        var parts = new List<string>();
                        for (int i = 0; i < fillers.Count; i++)
                        {
                            parts.Add(fillers[i]);
                            if (i < occurrences)
                                parts.Add(keyword);
                        }
                        return (Content: string.Concat(parts), Keyword: keyword);
                    })));

        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Generates arbitrary non-empty keyword strings for general search testing.
    /// </summary>
    public static Arbitrary<(string Content, string Keyword)> RandomContentAndKeywordArbitrary()
    {
        var gen = Gen.Elements("a", "ab", "test", "XX", "hello", "the")
            .SelectMany(keyword =>
                Gen.Elements(
                    $"prefix {keyword} suffix",
                    $"{keyword}{keyword}{keyword}",
                    $"no match here at all",
                    $"start {keyword} middle {keyword} end",
                    $"{keyword.ToUpper()} mixed {keyword.ToLower()} case")
                .Select(content => (Content: content, Keyword: keyword)));

        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Property 7: TotalMatches equals the actual number of case-insensitive occurrences
    /// of the keyword in the content.
    /// **Validates: Requirements 6.2, 6.3**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(SearchResultPropertyTests) })]
    public void TotalMatches_Equals_ActualOccurrenceCount((string Content, string Keyword) input)
    {
        // Arrange: count occurrences independently using a simple loop
        var expectedCount = CountOccurrences(input.Content, input.Keyword);

        // Act
        var result = _sut.Search(input.Content, input.Keyword);

        // Assert
        result.TotalMatches.Should().Be(expectedCount,
            because: $"keyword '{input.Keyword}' should appear exactly {expectedCount} times in the content");
        result.MatchPositions.Should().HaveCount(expectedCount,
            because: "MatchPositions count should be consistent with TotalMatches");
    }

    /// <summary>
    /// Property 7: Each match position accurately points to the keyword's start index,
    /// meaning the substring at that position equals the keyword (case-insensitive).
    /// **Validates: Requirements 6.2, 6.3**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(SearchResultPropertyTests) })]
    public void MatchPositions_PointTo_KeywordStartIndex((string Content, string Keyword) input)
    {
        // Act
        var result = _sut.Search(input.Content, input.Keyword);

        // Assert: each position should point to a valid substring matching the keyword
        foreach (var position in result.MatchPositions)
        {
            position.Should().BeGreaterThanOrEqualTo(0,
                because: "match positions should be non-negative");
            position.Should().BeLessThanOrEqualTo(input.Content.Length - input.Keyword.Length,
                because: "match position plus keyword length should not exceed content length");

            var substringAtPosition = input.Content.Substring(position, input.Keyword.Length);
            substringAtPosition.Should().BeEquivalentTo(input.Keyword,
                because: $"the substring at position {position} should match the keyword (case-insensitive)");
        }
    }

    /// <summary>
    /// Property 7: Match positions are returned in ascending order and contain no duplicates.
    /// **Validates: Requirements 6.2, 6.3**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(SearchResultPropertyTests) })]
    public void MatchPositions_AreInAscendingOrder_WithNoDuplicates((string Content, string Keyword) input)
    {
        // Act
        var result = _sut.Search(input.Content, input.Keyword);

        // Assert: positions should be strictly ascending (no duplicates)
        result.MatchPositions.Should().BeInAscendingOrder(
            because: "match positions should be returned in document order");
        result.MatchPositions.Should().OnlyHaveUniqueItems(
            because: "each match position should be unique");
    }

    /// <summary>
    /// Property 7: Search is case-insensitive — searching with different casings of the
    /// same keyword yields the same match count and positions.
    /// **Validates: Requirements 6.2, 6.3**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(SearchResultPropertyTests) })]
    public void Search_IsCaseInsensitive_SameResultsRegardlessOfKeywordCase((string Content, string Keyword) input)
    {
        // Act
        var resultLower = _sut.Search(input.Content, input.Keyword.ToLower());
        var resultUpper = _sut.Search(input.Content, input.Keyword.ToUpper());

        // Assert: both should find the same matches
        resultLower.TotalMatches.Should().Be(resultUpper.TotalMatches,
            because: "case-insensitive search should return the same count regardless of keyword casing");
        resultLower.MatchPositions.Should().BeEquivalentTo(resultUpper.MatchPositions,
            because: "case-insensitive search should return the same positions regardless of keyword casing");
    }

    /// <summary>
    /// Independent occurrence counter for verification (case-insensitive, overlapping).
    /// </summary>
    private static int CountOccurrences(string content, string keyword)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keyword))
            return 0;

        int count = 0;
        int index = 0;
        while (index <= content.Length - keyword.Length)
        {
            int found = content.IndexOf(keyword, index, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
                break;
            count++;
            index = found + 1;
        }
        return count;
    }
}
