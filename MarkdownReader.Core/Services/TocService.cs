using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public partial class TocService : ITocService
{
    private readonly MarkdownPipeline _pipeline;

    public TocService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseMathematics()
            .Build();
    }

    public List<TocItem> GenerateToc(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return new List<TocItem>();

        var document = Markdig.Markdown.Parse(markdown, _pipeline);
        var flatItems = new List<TocItem>();

        foreach (var block in document)
        {
            if (block is HeadingBlock heading)
            {
                var title = ExtractText(heading.Inline);
                flatItems.Add(new TocItem
                {
                    Title = title,
                    Level = heading.Level,
                    AnchorId = GenerateAnchorId(title)
                });
            }
        }

        return BuildHierarchy(flatItems);
    }

    private static string ExtractText(ContainerInline? inline)
    {
        if (inline == null)
            return string.Empty;

        var text = string.Empty;
        foreach (var child in inline)
        {
            if (child is LiteralInline literal)
                text += literal.Content.ToString();
            else if (child is CodeInline code)
                text += code.Content;
            else if (child is EmphasisInline emphasis)
                text += ExtractText(emphasis);
            else if (child is LinkInline link)
                text += ExtractText(link);
        }

        return text;
    }

    private static string GenerateAnchorId(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        var id = title.ToLowerInvariant();
        id = id.Replace(' ', '-');
        id = SpecialCharsRegex().Replace(id, string.Empty);
        // Collapse multiple consecutive hyphens
        id = MultipleHyphensRegex().Replace(id, "-");
        id = id.Trim('-');
        return id;
    }

    private static List<TocItem> BuildHierarchy(List<TocItem> flatItems)
    {
        var roots = new List<TocItem>();
        var stack = new Stack<TocItem>();

        foreach (var item in flatItems)
        {
            // Pop items from stack that are at the same or deeper level
            while (stack.Count > 0 && stack.Peek().Level >= item.Level)
                stack.Pop();

            if (stack.Count == 0)
            {
                roots.Add(item);
            }
            else
            {
                stack.Peek().Children.Add(item);
            }

            stack.Push(item);
        }

        return roots;
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SpecialCharsRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
