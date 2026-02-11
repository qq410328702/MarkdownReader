namespace MarkdownReader.Core.Models;

public class SearchResult
{
    /// <summary>匹配项总数</summary>
    public int TotalMatches { get; set; }

    /// <summary>各匹配项的位置索引</summary>
    public List<int> MatchPositions { get; set; } = new();

    /// <summary>搜索关键词</summary>
    public string Keyword { get; set; } = string.Empty;
}
