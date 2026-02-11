using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;

namespace MarkdownReader.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IMarkdownService _markdownService;
    private readonly IThemeService _themeService;
    private readonly IExportService _exportService;
    private readonly IHtmlTemplateService _htmlTemplateService;
    private readonly ITocService _tocService;

    [ObservableProperty]
    private string _htmlContent = string.Empty;

    [ObservableProperty]
    private List<TocItem> _tocItems = new();

    [ObservableProperty]
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _windowTitle = "Markdown Reader";

    [ObservableProperty]
    private bool _isSearchVisible;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// Search ViewModel exposed for data binding.
    /// </summary>
    public SearchViewModel Search { get; } = new();

    /// <summary>
    /// Stores the raw markdown text of the currently loaded file (used for search).
    /// </summary>
    private string _currentMarkdown = string.Empty;

    public MainViewModel()
        : this(
            new FileService(),
            new MarkdownService(),
            new ThemeService(),
            new ExportService(),
            new HtmlTemplateService(),
            new TocService())
    {
    }

    public MainViewModel(
        IFileService fileService,
        IMarkdownService markdownService,
        IThemeService themeService,
        IExportService exportService,
        IHtmlTemplateService htmlTemplateService,
        ITocService tocService)
    {
        _fileService = fileService;
        _markdownService = markdownService;
        _themeService = themeService;
        _exportService = exportService;
        _htmlTemplateService = htmlTemplateService;
        _tocService = tocService;

        _themeService.LoadSavedTheme();
        _themeService.ThemeChanged += OnThemeChanged;
    }

    /// <summary>
    /// Open file via dialog (Ctrl+O).
    /// </summary>
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var path = _fileService.ShowOpenFileDialog();
        if (path is null) return;

        await LoadFileAsync(path);
    }

    /// <summary>
    /// Toggle search bar visibility (Ctrl+F).
    /// </summary>
    [RelayCommand]
    private void ShowSearch()
    {
        IsSearchVisible = !IsSearchVisible;
    }

    /// <summary>
    /// Toggle theme between Light and Dark.
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        // Re-render current content with new theme if a file is loaded
        if (!string.IsNullOrEmpty(_currentMarkdown))
        {
            var htmlFragment = _markdownService.ConvertToHtml(_currentMarkdown);
            HtmlContent = _htmlTemplateService.BuildFullHtml(htmlFragment, _themeService.CurrentTheme);
        }
    }

    /// <summary>
    /// Export current document as HTML (Ctrl+E).
    /// </summary>
    [RelayCommand]
    private async Task ExportHtmlAsync()
    {
        if (string.IsNullOrEmpty(HtmlContent)) return;

        var path = _fileService.ShowSaveFileDialog("HTML Files (*.html)|*.html", ".html");
        if (path is null) return;

        try
        {
            await _exportService.ExportToHtmlAsync(HtmlContent, path);
        }
        catch (Exception ex)
        {
            ShowError($"导出 HTML 失败：{ex.Message}");
        }
    }

    /// <summary>
    /// Loads and renders a markdown file from the given path.
    /// Called by OpenFile command, drag-drop, and recent file clicks.
    /// </summary>
    public async Task LoadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        ClearError();
        IsLoading = true;
        try
        {
            _currentMarkdown = await _fileService.ReadFileAsync(filePath);
            _fileService.AddRecentFile(filePath);

            CurrentFilePath = filePath;
            WindowTitle = $"{Path.GetFileName(filePath)} - Markdown Reader";

            var htmlFragment = _markdownService.ConvertToHtml(_currentMarkdown);
            TocItems = _tocService.GenerateToc(_currentMarkdown);
            HtmlContent = _htmlTemplateService.BuildFullHtml(htmlFragment, _themeService.CurrentTheme);
        }
        catch (Exception ex)
        {
            ShowError($"无法打开文件：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle drag-drop of a file onto the window.
    /// </summary>
    public async Task HandleFileDropAsync(string filePath)
    {
        await LoadFileAsync(filePath);
    }

    /// <summary>
    /// Re-render when theme changes externally.
    /// </summary>
    private void OnThemeChanged(object? sender, ThemeType newTheme)
    {
        if (!string.IsNullOrEmpty(_currentMarkdown))
        {
            var htmlFragment = _markdownService.ConvertToHtml(_currentMarkdown);
            HtmlContent = _htmlTemplateService.BuildFullHtml(htmlFragment, newTheme);
        }
    }

    /// <summary>
    /// Dismiss the current error notification.
    /// </summary>
    [RelayCommand]
    private void DismissError()
    {
        ClearError();
    }

    /// <summary>
    /// Set an error message and make the error bar visible.
    /// </summary>
    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    /// <summary>
    /// Clear the current error state.
    /// </summary>
    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }
}
