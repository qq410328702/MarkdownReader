namespace MarkdownReader.Core.Models;

public class TocItem
{
    /// <summary>标题文本</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>标题级别（1-6）</summary>
    public int Level { get; set; }

    /// <summary>用于锚点跳转的 ID</summary>
    public string AnchorId { get; set; } = string.Empty;

    /// <summary>子目录项</summary>
    public List<TocItem> Children { get; set; } = new();
}
