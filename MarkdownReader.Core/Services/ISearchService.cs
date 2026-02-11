using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public interface ISearchService
{
    /// <summary>
    /// 在文本中搜索关键词，返回所有匹配结果。
    /// </summary>
    SearchResult Search(string content, string keyword);
}
