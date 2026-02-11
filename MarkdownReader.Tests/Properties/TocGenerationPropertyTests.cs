using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 6: 目录生成正确性
/// For any 包含标题元素的 Markdown 文档，TOC_Generator 生成的目录树应包含文档中所有标题，
/// 且每个目录项的层级（Level）与原始标题级别一致，目录项的顺序与文档中标题出现的顺序一致。
/// **Validates: Requirements 5.1**
/// </summary>
public class TocGenerationPropertyTests
{
    private readonly TocService _sut = new();

    /// <summary>
    /// Represents a heading with its level and text, used to build markdown and verify TOC output.
    /// </summary>
    public record HeadingSpec(int Level, string Text);

    /// <summary>
    /// Generates a list of heading specs (level 1-6 with alphanumeric text),
    /// then builds a markdown string from them.
    /// Returns (headings, markdown) so we can verify the TOC against the original headings.
    /// </summary>
    public static Arbitrary<(List<HeadingSpec> Headings, string Markdown)> MarkdownWithHeadingsArbitrary()
    {
        var headingTextGen = Gen.Elements(
            "Introduction", "Overview", "Setup", "Configuration",
            "Architecture", "Components", "Testing", "Deployment",
            "Summary", "Conclusion", "References", "Appendix",
            "Getting Started", "Advanced Topics", "Best Practices",
            "Troubleshooting", "Performance", "Security");

        var headingGen = Gen.Choose(1, 6).SelectMany(level =>
            headingTextGen.Select(text => new HeadingSpec(level, text)));

        var gen = Gen.Choose(1, 8).SelectMany(count =>
            Gen.ListOf(headingGen, count).Select(headings =>
            {
                var headingsList = headings.ToList();
                // Build markdown: each heading separated by a paragraph
                var lines = new List<string>();
                foreach (var h in headingsList)
                {
                    var prefix = new string('#', h.Level);
                    lines.Add($"{prefix} {h.Text}");
                    lines.Add("");
                    lines.Add("Some paragraph text here.");
                    lines.Add("");
                }

                var markdown = string.Join("\n", lines).TrimEnd();
                return (Headings: headingsList, Markdown: markdown);
            }));

        return Arb.ToArbitrary(gen);
    }


    /// <summary>
    /// Flattens a hierarchical TOC tree into a list preserving document order (pre-order traversal).
    /// </summary>
    private static List<TocItem> FlattenToc(List<TocItem> items)
    {
        var result = new List<TocItem>();
        foreach (var item in items)
        {
            result.Add(item);
            result.AddRange(FlattenToc(item.Children));
        }
        return result;
    }

    /// <summary>
    /// Property 6: The TOC contains all headings from the document — none are lost.
    /// **Validates: Requirements 5.1**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(TocGenerationPropertyTests) })]
    public void Toc_ContainsAllHeadings_FromDocument(
        (List<HeadingSpec> Headings, string Markdown) input)
    {
        // Act
        var toc = _sut.GenerateToc(input.Markdown);
        var flatToc = FlattenToc(toc);

        // Assert: TOC should have exactly as many items as headings in the input
        flatToc.Should().HaveCount(input.Headings.Count,
            because: "every heading in the document should appear in the TOC");

        // Assert: every heading title should appear in the TOC
        for (int i = 0; i < input.Headings.Count; i++)
        {
            flatToc[i].Title.Should().Be(input.Headings[i].Text,
                because: $"heading '{input.Headings[i].Text}' at position {i} should be in the TOC");
        }
    }

    /// <summary>
    /// Property 6: Each TOC item's Level matches the original heading level.
    /// **Validates: Requirements 5.1**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(TocGenerationPropertyTests) })]
    public void Toc_ItemLevels_MatchOriginalHeadingLevels(
        (List<HeadingSpec> Headings, string Markdown) input)
    {
        // Act
        var toc = _sut.GenerateToc(input.Markdown);
        var flatToc = FlattenToc(toc);

        // Assert: each TOC item's level should match the corresponding heading level
        flatToc.Should().HaveCount(input.Headings.Count);
        for (int i = 0; i < input.Headings.Count; i++)
        {
            flatToc[i].Level.Should().Be(input.Headings[i].Level,
                because: $"TOC item at position {i} should have level {input.Headings[i].Level}");
        }
    }

    /// <summary>
    /// Property 6: The order of items in the flattened TOC matches document order.
    /// **Validates: Requirements 5.1**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(TocGenerationPropertyTests) })]
    public void Toc_ItemOrder_MatchesDocumentOrder(
        (List<HeadingSpec> Headings, string Markdown) input)
    {
        // Act
        var toc = _sut.GenerateToc(input.Markdown);
        var flatToc = FlattenToc(toc);

        // Assert: the sequence of (Title, Level) pairs should match the input headings exactly
        var expectedSequence = input.Headings
            .Select(h => (h.Text, h.Level))
            .ToList();

        var actualSequence = flatToc
            .Select(t => (t.Title, t.Level))
            .ToList();

        actualSequence.Should().Equal(expectedSequence,
            because: "TOC items should appear in the same order as headings in the document");
    }
}
