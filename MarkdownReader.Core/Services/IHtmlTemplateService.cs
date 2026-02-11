using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public interface IHtmlTemplateService
{
    /// <summary>
    /// 将 Markdown 转换后的 HTML 片段包装为完整的 HTML 页面。
    /// 包含 KaTeX 自动渲染、Mermaid.js 初始化、highlight.js 和主题样式。
    /// </summary>
    string BuildFullHtml(string htmlFragment, ThemeType theme);
}
