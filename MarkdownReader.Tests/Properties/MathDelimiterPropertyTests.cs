using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 4: 数学公式定界符保留
/// For any 包含行内数学公式（单美元符号包裹）或块级数学公式（双美元符号包裹）的 Markdown 文本，
/// 解析后的 HTML 应保留数学公式的内容，使其可被 KaTeX 自动渲染脚本识别和处理。
/// **Validates: Requirements 2.1, 2.2**
/// </summary>
public class MathDelimiterPropertyTests
{
    private readonly MarkdownService _sut = new();

    /// <summary>
    /// Generates random LaTeX math expressions for property testing.
    /// </summary>
    private static Gen<string> LatexExpressionGen()
    {
        return Gen.OneOf(
            Gen.Elements(
                "E=mc^2",
                "x^2+y^2=z^2",
                @"\frac{a}{b}",
                @"\sum_{i=1}^{n} i",
                @"\int_0^1 x\,dx",
                @"\alpha + \beta",
                @"\sqrt{x}",
                @"\mathbb{R}",
                "a^2 + b^2 = c^2",
                @"\lim_{n\to\infty} a_n",
                @"\binom{n}{k}",
                "f(x) = ax + b"
            ),
            // Generate simple variable expressions like "x_1", "y_2"
            Gen.Elements("x", "y", "z", "a", "b").SelectMany(
                v => Gen.Choose(1, 9).Select(n => v + "_" + n))
        );
    }

    /// <summary>
    /// Generates a tuple of (latex expression, markdown with inline math).
    /// </summary>
    public static Arbitrary<(string Latex, string Markdown)> InlineMathWithExprArbitrary()
    {
        var gen = LatexExpressionGen()
            .Select(expr => (expr, "Text with $" + expr + "$ inline."));
        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Generates a tuple of (latex expression, markdown with block math).
    /// </summary>
    public static Arbitrary<(string Latex, string Markdown)> BlockMathWithExprArbitrary()
    {
        var gen = LatexExpressionGen()
            .Select(expr => (expr, "Before block.\n\n$$\n" + expr + "\n$$\n\nAfter block."));
        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Property 4: Inline math content is preserved in HTML output within a math span element.
    /// Markdig's Mathematics extension wraps inline math in &lt;span class="math"&gt;.
    /// **Validates: Requirements 2.1**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MathDelimiterPropertyTests) })]
    public void InlineMath_ContentIsPreserved_InHtmlOutput((string Latex, string Markdown) input)
    {
        // Act
        var html = _sut.ConvertToHtml(input.Markdown);

        // Assert: HTML should contain a span with class "math" for inline math
        html.Should().Contain("class=\"math\"",
            because: "inline math ($...$) should be wrapped in a math-classed element by Markdig");

        // Assert: the LaTeX expression content should be preserved in the HTML
        html.Should().Contain(input.Latex,
            because: "the math expression should be preserved in the HTML for KaTeX to render");
    }

    /// <summary>
    /// Property 4: Block math content is preserved in HTML output within a math div element.
    /// Markdig's Mathematics extension wraps block math in &lt;div class="math"&gt;.
    /// **Validates: Requirements 2.2**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MathDelimiterPropertyTests) })]
    public void BlockMath_ContentIsPreserved_InHtmlOutput((string Latex, string Markdown) input)
    {
        // Act
        var html = _sut.ConvertToHtml(input.Markdown);

        // Assert: HTML should contain a div with class "math" for block math
        html.Should().Contain("class=\"math\"",
            because: "block math ($$...$$) should be wrapped in a math-classed element by Markdig");

        // Assert: the LaTeX expression content should be preserved in the HTML
        html.Should().Contain(input.Latex,
            because: "the math expression should be preserved in the HTML for KaTeX to render");
    }

    /// <summary>
    /// Property 4: Inline math uses span element, block math uses div element —
    /// the correct HTML element type distinguishes inline vs block math.
    /// **Validates: Requirements 2.1, 2.2**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(MathDelimiterPropertyTests) })]
    public void InlineMath_UsesSpan_BlockMath_UsesDiv((string Latex, string Markdown) inlineInput)
    {
        // Test inline math produces <span>
        var inlineHtml = _sut.ConvertToHtml(inlineInput.Markdown);
        inlineHtml.Should().Contain("<span class=\"math\"",
            because: "inline math should be rendered as a <span> element");

        // Test block math produces <div>
        var blockMarkdown = "Before.\n\n$$\n" + inlineInput.Latex + "\n$$\n\nAfter.";
        var blockHtml = _sut.ConvertToHtml(blockMarkdown);
        blockHtml.Should().Contain("<div class=\"math\"",
            because: "block math should be rendered as a <div> element");
    }
}
