namespace MarkdownReader.Core.Services;

public interface IMarkdownService
{
    /// <summary>
    /// 将 Markdown 文本转换为 HTML 片段。
    /// 启用 GFM 扩展、数学公式扩展和代码高亮 CSS 类。
    /// </summary>
    string ConvertToHtml(string markdown);

    /// <summary>
    /// 将 Markdown 文本转换为纯文本（用于搜索）。
    /// </summary>
    string ConvertToPlainText(string markdown);
}
