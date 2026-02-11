using FluentAssertions;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests.Services;

public class HtmlTemplateServiceTests
{
    private readonly HtmlTemplateService _service = new();

    // --- KaTeX references ---

    [Fact]
    public void BuildFullHtml_ContainsKaTexCssReference()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("katex.min.css");
    }

    [Fact]
    public void BuildFullHtml_ContainsKaTexJsReferences()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("katex.min.js");
        html.Should().Contain("auto-render.min.js");
    }

    [Fact]
    public void BuildFullHtml_ContainsKaTexAutoRenderInitialization()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("renderMathInElement");
    }

    // --- Mermaid.js references ---

    [Fact]
    public void BuildFullHtml_ContainsMermaidJsReference()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("mermaid.min.js");
    }

    [Fact]
    public void BuildFullHtml_ContainsMermaidInitialization()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("mermaid.initialize");
    }

    // --- highlight.js references ---

    [Fact]
    public void BuildFullHtml_ContainsHighlightJsReference()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("highlight.min.js");
    }

    [Fact]
    public void BuildFullHtml_ContainsHighlightJsInitialization()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("hljs.highlightAll()");
    }

    // --- Light theme ---

    [Fact]
    public void BuildFullHtml_LightTheme_AppliesLightBodyClass()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("body class=\"light\"");
    }

    [Fact]
    public void BuildFullHtml_LightTheme_UsesLightHighlightJsTheme()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("highlight.js/styles/github.min.css");
    }

    [Fact]
    public void BuildFullHtml_LightTheme_UsesMermaidDefaultTheme()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Light);

        html.Should().Contain("theme: 'default'");
    }

    // --- Dark theme ---

    [Fact]
    public void BuildFullHtml_DarkTheme_AppliesDarkBodyClass()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Dark);

        html.Should().Contain("body class=\"dark\"");
    }

    [Fact]
    public void BuildFullHtml_DarkTheme_UsesDarkHighlightJsTheme()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Dark);

        html.Should().Contain("highlight.js/styles/github-dark.min.css");
    }

    [Fact]
    public void BuildFullHtml_DarkTheme_UsesMermaidDarkTheme()
    {
        var html = _service.BuildFullHtml("<p>test</p>", ThemeType.Dark);

        html.Should().Contain("theme: 'dark'");
    }

    // --- HTML fragment embedding ---

    [Fact]
    public void BuildFullHtml_EmbedsHtmlFragmentInOutput()
    {
        var fragment = "<h1>Hello World</h1><p>Some content here</p>";

        var html = _service.BuildFullHtml(fragment, ThemeType.Light);

        html.Should().Contain(fragment);
    }

    [Fact]
    public void BuildFullHtml_EmbedsComplexHtmlFragment()
    {
        var fragment = "<pre><code class=\"language-csharp\">var x = 1;</code></pre>";

        var html = _service.BuildFullHtml(fragment, ThemeType.Dark);

        html.Should().Contain(fragment);
    }
}
