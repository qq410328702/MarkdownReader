using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public class HtmlTemplateService : IHtmlTemplateService
{
    public string BuildFullHtml(string htmlFragment, ThemeType theme)
    {
        var isDark = theme == ThemeType.Dark;
        var highlightTheme = isDark ? "github-dark" : "github";
        var mermaidTheme = isDark ? "dark" : "default";
        var themeClass = isDark ? "dark" : "light";

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/katex/dist/katex.min.css"">
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/highlight.js/styles/{highlightTheme}.min.css"">
    <style>
        body.light {{
            background-color: #ffffff;
            color: #24292e;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
            line-height: 1.6;
            padding: 20px;
            max-width: 900px;
            margin: 0 auto;
        }}
        body.dark {{
            background-color: #0d1117;
            color: #c9d1d9;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
            line-height: 1.6;
            padding: 20px;
            max-width: 900px;
            margin: 0 auto;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 16px 0;
        }}
        th, td {{
            border: 1px solid {(isDark ? "#30363d" : "#d0d7de")};
            padding: 8px 12px;
            text-align: left;
        }}
        th {{
            background-color: {(isDark ? "#161b22" : "#f6f8fa")};
        }}
        pre {{
            background-color: {(isDark ? "#161b22" : "#f6f8fa")};
            border-radius: 6px;
            padding: 16px;
            overflow: auto;
        }}
        code {{
            font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
            font-size: 85%;
        }}
        blockquote {{
            border-left: 4px solid {(isDark ? "#30363d" : "#d0d7de")};
            color: {(isDark ? "#8b949e" : "#57606a")};
            padding: 0 16px;
            margin: 16px 0;
        }}
        a {{
            color: {(isDark ? "#58a6ff" : "#0969da")};
        }}
        img {{
            max-width: 100%;
        }}
        hr {{
            border: none;
            border-top: 1px solid {(isDark ? "#30363d" : "#d0d7de")};
            margin: 24px 0;
        }}
    </style>
</head>
<body class=""{themeClass}"">
    {htmlFragment}

    <script src=""https://cdn.jsdelivr.net/npm/katex/dist/katex.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/katex/dist/contrib/auto-render.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/highlight.js/lib/highlight.min.js""></script>
    <script>
        renderMathInElement(document.body, {{
            delimiters: [
                {{left: ""$$"", right: ""$$"", display: true}},
                {{left: ""$"", right: ""$"", display: false}}
            ]
        }});
        mermaid.initialize({{ theme: '{mermaidTheme}' }});
        hljs.highlightAll();
    </script>
</body>
</html>";
    }
}
