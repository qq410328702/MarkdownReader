using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 2: Markdown 元素到 HTML 标签的正确映射
/// For any 包含标准 Markdown 语法元素或 GFM 扩展语法元素的 Markdown 文本，
/// 解析后的 HTML 应包含与每种元素对应的正确 HTML 标签。
/// **Validates: Requirements 1.2, 1.3, 1.4**
/// </summary>
public class MarkdownElementMappingPropertyTests
{
    private readonly MarkdownService _sut = new();

    /// <summary>
    /// Record representing a Markdown element and its expected HTML tag.
    /// </summary>
    public record MarkdownElementCase(string Markdown, string ExpectedHtmlTag, string Description);

    /// <summary>
    /// Generates arbitrary MarkdownElementCase instances covering standard Markdown
    /// and GFM extension elements.
    /// </summary>
    public static Arbitrary<MarkdownElementCase> MarkdownElementArbitrary()
    {
        var safeWords = Gen.Elements(
            "Alpha", "Bravo", "Charlie", "Delta", "Echo",
            "Foxtrot", "Golf", "Hotel", "India", "Juliet");

        // Headings: # through ###### (Markdig adds id attributes, so match tag prefix)
        var headingGen = Gen.Choose(1, 6).SelectMany(level =>
            safeWords.Select(word =>
                new MarkdownElementCase(
                    new string('#', level) + " " + word,
                    $"<h{level}",
                    $"H{level} heading")));

        // Bold: **text**
        var boldGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"**{word}**",
                "<strong>",
                "Bold text"));

        // Italic: *text*
        var italicGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"*{word}*",
                "<em>",
                "Italic text"));

        // Inline code: `code`
        var inlineCodeGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"`{word}`",
                "<code>",
                "Inline code"));

        // Fenced code block with language
        var languages = Gen.Elements("csharp", "python", "javascript", "java", "go", "rust");
        var fencedCodeGen = languages.SelectMany(lang =>
            safeWords.Select(word =>
                new MarkdownElementCase(
                    $"```{lang}\n{word}\n```",
                    $"language-{lang}",
                    $"Fenced code block with {lang}")));

        // Strikethrough (GFM): ~~text~~
        var strikethroughGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"~~{word}~~",
                "<del>",
                "Strikethrough"));

        // Task list (GFM): - [x] text / - [ ] text
        var taskListGen = Gen.Elements(true, false).SelectMany(isChecked =>
            safeWords.Select(word =>
            {
                var check = isChecked ? "x" : " ";
                return new MarkdownElementCase(
                    $"- [{check}] {word}",
                    "<input",
                    "Task list item");
            }));

        // Table (GFM)
        var tableGen = safeWords.SelectMany(w1 =>
            safeWords.Select(w2 =>
                new MarkdownElementCase(
                    $"| {w1} | {w2} |\n| --- | --- |\n| val1 | val2 |",
                    "<table>",
                    "Table")));

        // Blockquote: > text
        var blockquoteGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"> {word}",
                "<blockquote>",
                "Blockquote"));

        // Horizontal rule: ---
        var hrGen = FsCheck.Fluent.Gen.Constant(
            new MarkdownElementCase("---", "<hr", "Horizontal rule"));

        // Link: [text](url)
        var linkGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"[{word}](https://example.com)",
                "<a",
                "Link"));

        // Image: ![alt](url)
        var imageGen = safeWords.Select(word =>
            new MarkdownElementCase(
                $"![{word}](https://example.com/img.png)",
                "<img",
                "Image"));

        var allGen = Gen.OneOf(
            headingGen, boldGen, italicGen, inlineCodeGen, fencedCodeGen,
            strikethroughGen, taskListGen, tableGen, blockquoteGen,
            hrGen, linkGen, imageGen);

        return Arb.ToArbitrary(allGen);
    }

    /// <summary>
    /// Property 2: Markdown 元素到 HTML 标签的正确映射
    /// For each generated Markdown element, the HTML output must contain
    /// the corresponding HTML tag.
    /// **Validates: Requirements 1.2, 1.3, 1.4**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MarkdownElementMappingPropertyTests) })]
    public void MarkdownElement_MapsTo_CorrectHtmlTag(MarkdownElementCase testCase)
    {
        // Act
        var html = _sut.ConvertToHtml(testCase.Markdown);

        // Assert
        html.Should().Contain(testCase.ExpectedHtmlTag,
            because: $"Markdown element '{testCase.Description}' should map to HTML containing '{testCase.ExpectedHtmlTag}'. " +
                     $"Input: {testCase.Markdown}");
    }
}
