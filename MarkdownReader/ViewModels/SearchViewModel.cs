using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MarkdownReader.ViewModels;

/// <summary>
/// ViewModel for the document search bar.
/// Manages search keyword, match count/index, and navigation.
/// Actual WebView2 JS highlighting is triggered via events consumed by the View.
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private int _currentMatchIndex;

    [ObservableProperty]
    private int _totalMatches;

    [ObservableProperty]
    private string _searchMatchText = "0/0";

    /// <summary>
    /// Raised when the view should execute a search highlight in WebView2.
    /// The string parameter is the keyword to highlight (empty to clear).
    /// </summary>
    public event EventHandler<string>? SearchRequested;

    /// <summary>
    /// Raised when the view should scroll to a specific match index in WebView2.
    /// </summary>
    public event EventHandler<int>? NavigateToMatchRequested;

    partial void OnSearchKeywordChanged(string value)
    {
        // Reset match state and request a new search
        CurrentMatchIndex = 0;
        TotalMatches = 0;
        UpdateMatchText();
        SearchRequested?.Invoke(this, value ?? string.Empty);
    }

    /// <summary>
    /// Called by the View after JS returns the match count.
    /// </summary>
    public void UpdateMatchCount(int count)
    {
        TotalMatches = count;
        CurrentMatchIndex = count > 0 ? 1 : 0;
        UpdateMatchText();

        if (count > 0)
        {
            NavigateToMatchRequested?.Invoke(this, CurrentMatchIndex - 1);
        }
    }

    [RelayCommand]
    private void SearchNext()
    {
        if (TotalMatches == 0) return;

        CurrentMatchIndex = CurrentMatchIndex >= TotalMatches ? 1 : CurrentMatchIndex + 1;
        UpdateMatchText();
        NavigateToMatchRequested?.Invoke(this, CurrentMatchIndex - 1);
    }

    [RelayCommand]
    private void SearchPrevious()
    {
        if (TotalMatches == 0) return;

        CurrentMatchIndex = CurrentMatchIndex <= 1 ? TotalMatches : CurrentMatchIndex - 1;
        UpdateMatchText();
        NavigateToMatchRequested?.Invoke(this, CurrentMatchIndex - 1);
    }

    /// <summary>
    /// Clears search state. Called when search bar is closed.
    /// </summary>
    public void ClearSearch()
    {
        SearchKeyword = string.Empty;
        CurrentMatchIndex = 0;
        TotalMatches = 0;
        UpdateMatchText();
    }

    private void UpdateMatchText()
    {
        SearchMatchText = TotalMatches > 0
            ? $"{CurrentMatchIndex}/{TotalMatches}"
            : "0/0";
    }
}
