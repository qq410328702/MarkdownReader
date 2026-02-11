using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public interface ITocService
{
    /// <summary>
    /// 从 Markdown 文本中提取标题，生成层级目录树。
    /// </summary>
    List<TocItem> GenerateToc(string markdown);
}
