using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public class SearchService : ISearchService
{
    public SearchResult Search(string content, string keyword)
    {
        var result = new SearchResult
        {
            Keyword = keyword ?? string.Empty
        };

        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keyword))
            return result;

        var positions = new List<int>();
        int index = 0;

        while (index <= content.Length - keyword.Length)
        {
            int found = content.IndexOf(keyword, index, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
                break;

            positions.Add(found);
            index = found + 1;
        }

        result.MatchPositions = positions;
        result.TotalMatches = positions.Count;

        return result;
    }
}
