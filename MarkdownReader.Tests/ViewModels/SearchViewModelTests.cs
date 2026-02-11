using FluentAssertions;
using MarkdownReader.ViewModels;

namespace MarkdownReader.Tests.ViewModels;

public class SearchViewModelTests
{
    [Fact]
    public void InitialState_HasDefaultValues()
    {
        var vm = new SearchViewModel();

        vm.SearchKeyword.Should().BeEmpty();
        vm.CurrentMatchIndex.Should().Be(0);
        vm.TotalMatches.Should().Be(0);
        vm.SearchMatchText.Should().Be("0/0");
    }

    [Fact]
    public void SearchKeyword_Change_RaisesSearchRequested()
    {
        var vm = new SearchViewModel();
        string? receivedKeyword = null;
        vm.SearchRequested += (_, kw) => receivedKeyword = kw;

        vm.SearchKeyword = "test";

        receivedKeyword.Should().Be("test");
    }

    [Fact]
    public void SearchKeyword_Change_ResetsMatchState()
    {
        var vm = new SearchViewModel();
        vm.UpdateMatchCount(5);
        vm.SearchNextCommand.Execute(null);

        vm.SearchKeyword = "new";

        vm.CurrentMatchIndex.Should().Be(0);
        vm.TotalMatches.Should().Be(0);
    }

    [Fact]
    public void UpdateMatchCount_WithMatches_SetsFirstMatch()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";

        vm.UpdateMatchCount(3);

        vm.TotalMatches.Should().Be(3);
        vm.CurrentMatchIndex.Should().Be(1);
        vm.SearchMatchText.Should().Be("1/3");
    }

    [Fact]
    public void UpdateMatchCount_Zero_KeepsIndexAtZero()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";

        vm.UpdateMatchCount(0);

        vm.TotalMatches.Should().Be(0);
        vm.CurrentMatchIndex.Should().Be(0);
        vm.SearchMatchText.Should().Be("0/0");
    }

    [Fact]
    public void UpdateMatchCount_RaisesNavigateToMatchRequested()
    {
        var vm = new SearchViewModel();
        int? navigatedIndex = null;
        vm.NavigateToMatchRequested += (_, idx) => navigatedIndex = idx;
        vm.SearchKeyword = "test";

        vm.UpdateMatchCount(3);

        navigatedIndex.Should().Be(0); // first match, zero-based
    }

    [Fact]
    public void SearchNext_AdvancesToNextMatch()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);

        vm.SearchNextCommand.Execute(null);

        vm.CurrentMatchIndex.Should().Be(2);
        vm.SearchMatchText.Should().Be("2/3");
    }

    [Fact]
    public void SearchNext_WrapsAroundToFirst()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(2);

        vm.SearchNextCommand.Execute(null); // 2/2
        vm.SearchNextCommand.Execute(null); // wraps to 1/2

        vm.CurrentMatchIndex.Should().Be(1);
        vm.SearchMatchText.Should().Be("1/2");
    }

    [Fact]
    public void SearchPrevious_GoesToPreviousMatch()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);
        vm.SearchNextCommand.Execute(null); // 2/3

        vm.SearchPreviousCommand.Execute(null);

        vm.CurrentMatchIndex.Should().Be(1);
        vm.SearchMatchText.Should().Be("1/3");
    }

    [Fact]
    public void SearchPrevious_WrapsAroundToLast()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);
        // CurrentMatchIndex is 1 after UpdateMatchCount

        vm.SearchPreviousCommand.Execute(null);

        vm.CurrentMatchIndex.Should().Be(3);
        vm.SearchMatchText.Should().Be("3/3");
    }

    [Fact]
    public void SearchNext_NoMatches_DoesNothing()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(0);

        vm.SearchNextCommand.Execute(null);

        vm.CurrentMatchIndex.Should().Be(0);
    }

    [Fact]
    public void SearchPrevious_NoMatches_DoesNothing()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(0);

        vm.SearchPreviousCommand.Execute(null);

        vm.CurrentMatchIndex.Should().Be(0);
    }

    [Fact]
    public void ClearSearch_ResetsAllState()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(5);
        vm.SearchNextCommand.Execute(null);

        vm.ClearSearch();

        vm.SearchKeyword.Should().BeEmpty();
        vm.CurrentMatchIndex.Should().Be(0);
        vm.TotalMatches.Should().Be(0);
        vm.SearchMatchText.Should().Be("0/0");
    }

    [Fact]
    public void SearchNext_RaisesNavigateToMatchRequested_WithCorrectIndex()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);

        int? navigatedIndex = null;
        vm.NavigateToMatchRequested += (_, idx) => navigatedIndex = idx;

        vm.SearchNextCommand.Execute(null); // moves to 2/3

        navigatedIndex.Should().Be(1); // zero-based index 1
    }

    [Fact]
    public void SearchPrevious_RaisesNavigateToMatchRequested_WithCorrectIndex()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);

        int? navigatedIndex = null;
        vm.NavigateToMatchRequested += (_, idx) => navigatedIndex = idx;

        vm.SearchPreviousCommand.Execute(null); // wraps to 3/3

        navigatedIndex.Should().Be(2); // zero-based index 2
    }

    [Fact]
    public void ClearSearch_RaisesSearchRequested_WithEmptyKeyword()
    {
        var vm = new SearchViewModel();
        vm.SearchKeyword = "test";
        vm.UpdateMatchCount(3);

        string? receivedKeyword = null;
        vm.SearchRequested += (_, kw) => receivedKeyword = kw;

        vm.ClearSearch();

        receivedKeyword.Should().BeEmpty();
    }
}
