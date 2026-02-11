using FluentAssertions;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests;

public class TocServiceTests
{
    private readonly TocService _sut = new();

    [Fact]
    public void GenerateToc_EmptyString_ReturnsEmptyList()
    {
        var result = _sut.GenerateToc("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToc_NullString_ReturnsEmptyList()
    {
        var result = _sut.GenerateToc(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToc_NoHeadings_ReturnsEmptyList()
    {
        var result = _sut.GenerateToc("Just a paragraph with no headings.");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToc_SingleH1_ReturnsSingleItem()
    {
        var result = _sut.GenerateToc("# Hello World");
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Hello World");
        result[0].Level.Should().Be(1);
        result[0].AnchorId.Should().Be("hello-world");
        result[0].Children.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToc_MultipleTopLevelHeadings_ReturnsFlat()
    {
        var md = "# First\n\n# Second\n\n# Third";
        var result = _sut.GenerateToc(md);
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("First");
        result[1].Title.Should().Be("Second");
        result[2].Title.Should().Be("Third");
    }

    [Fact]
    public void GenerateToc_NestedHeadings_BuildsHierarchy()
    {
        var md = "# Chapter 1\n\n## Section 1.1\n\n## Section 1.2\n\n# Chapter 2\n\n## Section 2.1";
        var result = _sut.GenerateToc(md);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Chapter 1");
        result[0].Children.Should().HaveCount(2);
        result[0].Children[0].Title.Should().Be("Section 1.1");
        result[0].Children[1].Title.Should().Be("Section 1.2");

        result[1].Title.Should().Be("Chapter 2");
        result[1].Children.Should().HaveCount(1);
        result[1].Children[0].Title.Should().Be("Section 2.1");
    }

    [Fact]
    public void GenerateToc_DeeplyNested_BuildsCorrectTree()
    {
        var md = "# H1\n\n## H2\n\n### H3\n\n#### H4";
        var result = _sut.GenerateToc(md);

        result.Should().HaveCount(1);
        result[0].Level.Should().Be(1);
        result[0].Children.Should().HaveCount(1);
        result[0].Children[0].Level.Should().Be(2);
        result[0].Children[0].Children.Should().HaveCount(1);
        result[0].Children[0].Children[0].Level.Should().Be(3);
        result[0].Children[0].Children[0].Children.Should().HaveCount(1);
        result[0].Children[0].Children[0].Children[0].Level.Should().Be(4);
    }

    [Fact]
    public void GenerateToc_AnchorId_LowercaseAndHyphens()
    {
        var result = _sut.GenerateToc("# Hello World");
        result[0].AnchorId.Should().Be("hello-world");
    }

    [Fact]
    public void GenerateToc_AnchorId_RemovesSpecialChars()
    {
        var result = _sut.GenerateToc("# What's New? (2024)");
        result[0].AnchorId.Should().NotContain("'").And.NotContain("?").And.NotContain("(").And.NotContain(")");
    }

    [Fact]
    public void GenerateToc_HeadingWithInlineCode_ExtractsText()
    {
        var result = _sut.GenerateToc("# Using `Console.WriteLine`");
        result[0].Title.Should().Contain("Console.WriteLine");
    }

    [Fact]
    public void GenerateToc_HeadingWithBold_ExtractsText()
    {
        var result = _sut.GenerateToc("# **Bold** Title");
        result[0].Title.Should().Be("Bold Title");
    }

    [Fact]
    public void GenerateToc_PreservesDocumentOrder()
    {
        var md = "## Second Level First\n\n# Top Level\n\n### Third Level";
        var result = _sut.GenerateToc(md);

        // The H2 comes first in the document, so it should be a root
        result[0].Title.Should().Be("Second Level First");
        result[1].Title.Should().Be("Top Level");
    }

    [Fact]
    public void GenerateToc_LevelJump_H1ToH3_NestsCorrectly()
    {
        var md = "# Title\n\n### Skipped H2";
        var result = _sut.GenerateToc(md);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Title");
        result[0].Children.Should().HaveCount(1);
        result[0].Children[0].Title.Should().Be("Skipped H2");
    }
}
