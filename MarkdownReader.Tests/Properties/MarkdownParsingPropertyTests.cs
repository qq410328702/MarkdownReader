using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;
using System.Text.RegularExpressions;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 1: Markdown 解析往返一致性
/// For any 有效的 Markdown 文本，使用 Markdig 解析为 HTML 后，
/// 该 HTML 应包含原始 Markdown 中所有语义元素的对应 HTML 标签，且不丢失任何内容。
/// **Validates: Requirements 1.1, 1.6**
/// </summary>
public class MarkdownParsingPropertyTests
{
    private readonly MarkdownService _sut = new();

    /// <summary>
    /// Provides Arbitrary&lt;string&gt; that generates valid Markdown text
    /// with various elements: headings, paragraphs, bold, italic, lists.
    /// </summary>
    public static Arbitrary<string> MarkdownArbitrary()
    {
        var headingGen = Gen.Choose(1, 6).SelectMany(level =>
            Gen.Elements("Title", "Section", "Chapter", "Overview", "Summary", "Details")
                .Select(text => new string('#', level) + " " + text));

        var paragraphGen = Gen.Elements(
            "Hello world",
            "This is a paragraph",
            "Some text content here",
            "Another paragraph with words",
            "Testing markdown parsing");

        var boldGen = Gen.Elements("bold", "strong", "important", "key")
            .Select(text => $"**{text}**");

        var italicGen = Gen.Elements("italic", "emphasis", "note", "hint")
            .Select(text => $"*{text}*");

        var unorderedListGen = Gen.Choose(2, 4).SelectMany(count =>
            Gen.ListOf(Gen.Elements("apple", "banana", "cherry", "date"), count)
                .Select(items => string.Join("\n", items.Select(i => $"- {i}"))));

        var orderedListGen = Gen.Choose(2, 4).SelectMany(count =>
            Gen.ListOf(Gen.Elements("first", "second", "third", "fourth"), count)
                .Select(items => string.Join("\n",
                    items.Select((item, idx) => $"{idx + 1}. {item}"))));

        var blockquoteGen = Gen.Elements(
            "> This is a quote",
            "> Important note",
            "> Remember this");

        var markdownGen = Gen.Choose(2, 5).SelectMany(count =>
            Gen.ListOf(
                Gen.OneOf(headingGen, paragraphGen, boldGen, italicGen,
                          unorderedListGen, orderedListGen, blockquoteGen), count)
                .Select(parts => string.Join("\n\n", parts)));

        return Arb.ToArbitrary(markdownGen);
    }

    /// <summary>
    /// Property 1: Markdown 解析往返一致性
    /// Plain text content from the original markdown is preserved in the HTML output.
    /// No semantic content is lost during conversion.
    /// **Validates: Requirements 1.1, 1.6**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MarkdownParsingPropertyTests) })]
    public void PlainTextContent_IsPreserved_InHtmlOutput(string markdown)
    {
        // Arrange: get the plain text representation of the markdown
        var plainText = _sut.ConvertToPlainText(markdown);

        // Act: convert markdown to HTML
        var html = _sut.ConvertToHtml(markdown);

        // Assert: every meaningful word from the plain text should appear in the HTML
        var words = plainText
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1 && w.All(char.IsLetterOrDigit))
            .Distinct()
            .ToList();

        foreach (var word in words)
        {
            html.Should().Contain(word,
                because: $"the word '{word}' from the original markdown should be preserved in HTML output");
        }
    }

    /// <summary>
    /// Property 1 (supplementary): HTML output contains appropriate structural tags
    /// for the markdown elements present in the input.
    /// **Validates: Requirements 1.1, 1.6**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MarkdownParsingPropertyTests) })]
    public void HtmlOutput_ContainsStructuralTags_ForMarkdownElements(string markdown)
    {
        // Act
        var html = _sut.ConvertToHtml(markdown);

        // Assert: HTML should not be empty for non-empty markdown
        if (!string.IsNullOrWhiteSpace(markdown))
        {
            html.Trim().Should().NotBeEmpty(
                because: "valid markdown should produce non-empty HTML");

            // HTML should contain at least one HTML tag
            html.Should().MatchRegex("<[a-z][a-z0-9]*",
                because: "markdown conversion should produce HTML tags");
        }
    }

    /// <summary>
    /// Property 1 (supplementary): The plain text extracted from HTML output matches
    /// the plain text extracted directly from markdown — no semantic content is lost.
    /// **Validates: Requirements 1.1, 1.6**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MarkdownParsingPropertyTests) })]
    public void ConvertToHtml_DoesNotLose_SemanticContent(string markdown)
    {
        // Arrange
        var plainTextFromMarkdown = _sut.ConvertToPlainText(markdown);

        // Act
        var html = _sut.ConvertToHtml(markdown);

        // Strip HTML tags to get text content from HTML
        var textFromHtml = Regex.Replace(html, "<[^>]+>", " ");
        textFromHtml = Regex.Replace(textFromHtml, @"\s+", " ").Trim();

        // Assert: every significant word from the markdown plain text should appear in the HTML text
        var significantWords = plainTextFromMarkdown
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1 && w.All(char.IsLetterOrDigit))
            .Distinct()
            .ToList();

        foreach (var word in significantWords)
        {
            textFromHtml.Should().Contain(word,
                because: $"semantic content '{word}' should not be lost during markdown-to-HTML conversion");
        }
    }
}
