using FluentAssertions;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests;

public class MarkdownServiceTests
{
    private readonly MarkdownService _sut = new();

    [Fact]
    public void ConvertToHtml_Heading_ReturnsH1Tag()
    {
        var html = _sut.ConvertToHtml("# Hello");
        html.Should().Contain("<h1").And.Contain("Hello</h1>");
    }

    [Fact]
    public void ConvertToHtml_Bold_ReturnsStrongTag()
    {
        var html = _sut.ConvertToHtml("**bold**");
        html.Should().Contain("<strong>bold</strong>");
    }

    [Fact]
    public void ConvertToHtml_Italic_ReturnsEmTag()
    {
        var html = _sut.ConvertToHtml("*italic*");
        html.Should().Contain("<em>italic</em>");
    }

    [Fact]
    public void ConvertToHtml_UnorderedList_ReturnsUlTag()
    {
        var html = _sut.ConvertToHtml("- item1\n- item2");
        html.Should().Contain("<ul>").And.Contain("<li>item1</li>");
    }

    [Fact]
    public void ConvertToHtml_FencedCodeBlock_ReturnsPreCodeTags()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = _sut.ConvertToHtml(md);
        html.Should().Contain("<pre>").And.Contain("<code");
        html.Should().Contain("language-csharp");
    }

    [Fact]
    public void ConvertToHtml_GfmTable_ReturnsTableTag()
    {
        var md = "| A | B |\n|---|---|\n| 1 | 2 |";
        var html = _sut.ConvertToHtml(md);
        html.Should().Contain("<table>").And.Contain("<td>1</td>");
    }

    [Fact]
    public void ConvertToHtml_GfmTaskList_ReturnsCheckboxInput()
    {
        var md = "- [x] done\n- [ ] todo";
        var html = _sut.ConvertToHtml(md);
        html.Should().Contain("type=\"checkbox\"");
    }

    [Fact]
    public void ConvertToHtml_GfmStrikethrough_ReturnsDelTag()
    {
        var md = "~~deleted~~";
        var html = _sut.ConvertToHtml(md);
        html.Should().Contain("<del>deleted</del>");
    }

    [Fact]
    public void ConvertToHtml_InlineMath_PreservesMathDelimiters()
    {
        var md = "The formula $E=mc^2$ is famous.";
        var html = _sut.ConvertToHtml(md);
        // Markdig Mathematics extension wraps inline math in <span class="math">
        html.Should().Contain("math");
        html.Should().Contain("E=mc^2");
    }

    [Fact]
    public void ConvertToHtml_BlockMath_PreservesMathContent()
    {
        var md = "$$\n\\sum_{i=1}^{n} i\n$$";
        var html = _sut.ConvertToHtml(md);
        html.Should().Contain("math");
    }

    [Fact]
    public void ConvertToHtml_EmptyString_ReturnsEmptyOrWhitespace()
    {
        var html = _sut.ConvertToHtml("");
        html.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ConvertToPlainText_StripsMarkdown()
    {
        var plain = _sut.ConvertToPlainText("# Hello **world**");
        plain.Should().Contain("Hello").And.Contain("world");
        plain.Should().NotContain("#").And.NotContain("**");
    }

    [Fact]
    public void ConvertToPlainText_EmptyString_ReturnsEmptyOrWhitespace()
    {
        var plain = _sut.ConvertToPlainText("");
        plain.Trim().Should().BeEmpty();
    }
}
