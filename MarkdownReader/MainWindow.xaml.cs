﻿﻿﻿using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MarkdownReader.Core.Services;
using MarkdownReader.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace MarkdownReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IExportService _exportService = new ExportService();
    private readonly IFileService _fileService = new FileService();
    private bool _webViewReady;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Watch HtmlContent changes to push to WebView2
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Wire keyboard shortcuts to ViewModel commands
        OpenFileKeyBinding.Command = _viewModel.OpenFileCommand;
        SearchKeyBinding.Command = _viewModel.ShowSearchCommand;
        ExportKeyBinding.Command = _viewModel.ExportHtmlCommand;

        // Wire menu items
        MenuOpen.Click += (_, _) => _viewModel.OpenFileCommand.Execute(null);
        MenuExportHtml.Click += (_, _) => _viewModel.ExportHtmlCommand.Execute(null);
        MenuExportPdf.Click += OnExportPdfClick;
        MenuToggleTheme.Click += (_, _) => _viewModel.ToggleThemeCommand.Execute(null);

        // Search bar close
        SearchClose.Click += (_, _) => _viewModel.IsSearchVisible = false;

        // Wire search ViewModel events to WebView2 JS interop
        _viewModel.Search.SearchRequested += OnSearchRequested;
        _viewModel.Search.NavigateToMatchRequested += OnNavigateToMatch;

        // Recent files menu
        MenuRecentFiles.SubmenuOpened += (_, _) => PopulateRecentFilesMenu();

        // Menu exit
        MenuExit.Click += (_, _) => Close();

        // Drag-drop support
        AllowDrop = true;
        Drop += OnFileDrop;
        DragOver += OnDragOver;

        // Initialize WebView2
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await ContentWebView.EnsureCoreWebView2Async();
            _webViewReady = true;

            // If content was set before WebView2 was ready, render it now
            if (!string.IsNullOrEmpty(_viewModel.HtmlContent))
            {
                ContentWebView.NavigateToString(_viewModel.HtmlContent);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"WebView2 初始化失败，请确保已安装 WebView2 Runtime。\n\n{ex.Message}",
                "初始化错误",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.HtmlContent):
                if (_webViewReady && !string.IsNullOrEmpty(_viewModel.HtmlContent))
                {
                    ContentWebView.NavigateToString(_viewModel.HtmlContent);
                }
                break;

            case nameof(MainViewModel.IsSearchVisible):
                SearchBar.Visibility = _viewModel.IsSearchVisible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                if (_viewModel.IsSearchVisible)
                {
                    SearchTextBox.Focus();
                }
                else
                {
                    // Clear highlights when search bar is closed (Req 6.4)
                    _viewModel.Search.ClearSearch();
                    ClearWebViewHighlights();
                }
                break;

            case nameof(MainViewModel.WindowTitle):
                Title = _viewModel.WindowTitle;
                break;
        }
    }

    private async void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (!_webViewReady || string.IsNullOrEmpty(_viewModel.HtmlContent)) return;

        var path = _fileService.ShowSaveFileDialog("PDF Files (*.pdf)|*.pdf", ".pdf");
        if (path is null) return;

        try
        {
            await _exportService.ExportToPdfAsync(ContentWebView, path);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"导出 PDF 失败：{ex.Message}";
            _viewModel.HasError = true;
        }
    }

    private async void OnFileDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files is null || files.Length == 0) return;

        var filePath = files[0];
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext is ".md" or ".markdown")
        {
            await _viewModel.HandleFileDropAsync(filePath);
        }
    }

    private async void TocTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!_webViewReady) return;
        if (e.NewValue is not MarkdownReader.Core.Models.TocItem tocItem) return;
        if (string.IsNullOrEmpty(tocItem.AnchorId)) return;

        var escapedId = tocItem.AnchorId.Replace("'", "\\'");
        var script = $"document.getElementById('{escapedId}')?.scrollIntoView({{behavior: 'smooth'}})";
        await ContentWebView.ExecuteScriptAsync(script);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files is not null && files.Length > 0)
            {
                var ext = Path.GetExtension(files[0]).ToLowerInvariant();
                if (ext is ".md" or ".markdown")
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// Dynamically populates the "最近文件" submenu with recent file paths.
    /// </summary>
    private void PopulateRecentFilesMenu()
    {
        MenuRecentFiles.Items.Clear();

        var recentFiles = _fileService.GetRecentFiles();

        if (recentFiles.Count == 0)
        {
            MenuRecentFiles.Items.Add(new System.Windows.Controls.MenuItem
            {
                Header = "(空)",
                IsEnabled = false
            });
            return;
        }

        foreach (var filePath in recentFiles)
        {
            var menuItem = new System.Windows.Controls.MenuItem
            {
                Header = filePath
            };
            var path = filePath; // capture for closure
            menuItem.Click += async (_, _) => await _viewModel.LoadFileAsync(path);
            MenuRecentFiles.Items.Add(menuItem);
        }
    }


    #region Search WebView2 JavaScript Interop

    /// <summary>
    /// JavaScript functions injected into WebView2 for search highlighting.
    /// </summary>
    private const string SearchScript = """
        (function() {
            window.__searchClear = function() {
                var marks = document.querySelectorAll('mark.search-highlight');
                marks.forEach(function(m) {
                    var parent = m.parentNode;
                    parent.replaceChild(document.createTextNode(m.textContent), m);
                    parent.normalize();
                });
            };

            window.__searchHighlight = function(keyword) {
                window.__searchClear();
                if (!keyword) return 0;

                var count = 0;
                var walker = document.createTreeWalker(
                    document.body,
                    NodeFilter.SHOW_TEXT,
                    null,
                    false
                );

                var textNodes = [];
                while (walker.nextNode()) {
                    textNodes.push(walker.currentNode);
                }

                var lowerKeyword = keyword.toLowerCase();
                textNodes.forEach(function(node) {
                    var text = node.textContent;
                    var lowerText = text.toLowerCase();
                    var idx = lowerText.indexOf(lowerKeyword);
                    if (idx < 0) return;

                    var frag = document.createDocumentFragment();
                    var lastIdx = 0;
                    while (idx >= 0) {
                        if (idx > lastIdx) {
                            frag.appendChild(document.createTextNode(text.substring(lastIdx, idx)));
                        }
                        var mark = document.createElement('mark');
                        mark.className = 'search-highlight';
                        mark.style.backgroundColor = '#FFEB3B';
                        mark.style.color = '#000';
                        mark.style.padding = '0 1px';
                        mark.style.borderRadius = '2px';
                        mark.textContent = text.substring(idx, idx + keyword.length);
                        frag.appendChild(mark);
                        count++;
                        lastIdx = idx + keyword.length;
                        idx = lowerText.indexOf(lowerKeyword, lastIdx);
                    }
                    if (lastIdx < text.length) {
                        frag.appendChild(document.createTextNode(text.substring(lastIdx)));
                    }
                    node.parentNode.replaceChild(frag, node);
                });

                return count;
            };

            window.__searchScrollTo = function(index) {
                var marks = document.querySelectorAll('mark.search-highlight');
                marks.forEach(function(m) {
                    m.style.backgroundColor = '#FFEB3B';
                });
                if (index >= 0 && index < marks.length) {
                    marks[index].style.backgroundColor = '#FF9800';
                    marks[index].scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            };
        })();
        """;

    private async void OnSearchRequested(object? sender, string keyword)
    {
        if (!_webViewReady) return;

        try
        {
            // Inject search functions if not already present, then execute search
            await ContentWebView.ExecuteScriptAsync(SearchScript);

            if (string.IsNullOrEmpty(keyword))
            {
                await ContentWebView.ExecuteScriptAsync("window.__searchClear()");
                _viewModel.Search.UpdateMatchCount(0);
            }
            else
            {
                var escapedKeyword = keyword.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
                var result = await ContentWebView.ExecuteScriptAsync($"window.__searchHighlight('{escapedKeyword}')");

                if (int.TryParse(result, out var count))
                {
                    _viewModel.Search.UpdateMatchCount(count);
                }
                else
                {
                    _viewModel.Search.UpdateMatchCount(0);
                }
            }
        }
        catch
        {
            // WebView2 may not be ready or page not loaded
        }
    }

    private async void OnNavigateToMatch(object? sender, int matchIndex)
    {
        if (!_webViewReady) return;

        try
        {
            await ContentWebView.ExecuteScriptAsync($"window.__searchScrollTo({matchIndex})");
        }
        catch
        {
            // WebView2 may not be ready
        }
    }

    private async void ClearWebViewHighlights()
    {
        if (!_webViewReady) return;

        try
        {
            await ContentWebView.ExecuteScriptAsync(SearchScript);
            await ContentWebView.ExecuteScriptAsync("window.__searchClear()");
        }
        catch
        {
            // WebView2 may not be ready
        }
    }

    #endregion
}
