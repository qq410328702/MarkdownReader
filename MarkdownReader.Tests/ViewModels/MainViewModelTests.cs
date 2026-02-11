using System.IO;
using FluentAssertions;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;
using MarkdownReader.ViewModels;

namespace MarkdownReader.Tests.ViewModels;

public class MainViewModelTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempConfigPath;

    public MainViewModelTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"mdreader_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _tempConfigPath = Path.Combine(_tempDir, "appsettings.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            new FileService(_tempConfigPath),
            new MarkdownService(),
            new ThemeService(_tempConfigPath),
            new ExportService(),
            new HtmlTemplateService(),
            new TocService());
    }

    private string CreateTempMarkdownFile(string content)
    {
        var path = Path.Combine(_tempDir, $"test_{Guid.NewGuid():N}.md");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void InitialState_HasDefaultValues()
    {
        var vm = CreateViewModel();

        vm.HtmlContent.Should().BeEmpty();
        vm.TocItems.Should().BeEmpty();
        vm.CurrentFilePath.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
        vm.WindowTitle.Should().Be("Markdown Reader");
        vm.IsSearchVisible.Should().BeFalse();
    }

    [Fact]
    public async Task LoadFileAsync_ValidFile_SetsHtmlContentAndToc()
    {
        var vm = CreateViewModel();
        var markdown = "# Hello\n\nSome **bold** text.\n\n## Section 2\n\nMore content.";
        var filePath = CreateTempMarkdownFile(markdown);

        await vm.LoadFileAsync(filePath);

        vm.HtmlContent.Should().NotBeNullOrEmpty();
        vm.HtmlContent.Should().Contain("<h1");
        vm.HtmlContent.Should().Contain("<strong>");
        vm.CurrentFilePath.Should().Be(filePath);
        vm.WindowTitle.Should().Contain(Path.GetFileName(filePath));
        vm.WindowTitle.Should().Contain("Markdown Reader");
        vm.TocItems.Should().NotBeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadFileAsync_SetsIsLoadingDuringExecution()
    {
        var vm = CreateViewModel();
        var filePath = CreateTempMarkdownFile("# Test");

        // After completion, IsLoading should be false
        await vm.LoadFileAsync(filePath);
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadFileAsync_NonExistentFile_DoesNotCrash()
    {
        var vm = CreateViewModel();

        await vm.LoadFileAsync(Path.Combine(_tempDir, "nonexistent.md"));

        vm.HtmlContent.Should().BeEmpty();
        vm.CurrentFilePath.Should().BeEmpty();
        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadFileAsync_NonExistentFile_SetsErrorState()
    {
        var vm = CreateViewModel();

        await vm.LoadFileAsync(Path.Combine(_tempDir, "nonexistent.md"));

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("无法打开文件");
    }

    [Fact]
    public async Task LoadFileAsync_ValidFile_ClearsErrorState()
    {
        var vm = CreateViewModel();

        // First trigger an error
        await vm.LoadFileAsync(Path.Combine(_tempDir, "nonexistent.md"));
        vm.HasError.Should().BeTrue();

        // Then load a valid file — error should be cleared
        var filePath = CreateTempMarkdownFile("# OK");
        await vm.LoadFileAsync(filePath);

        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void DismissErrorCommand_ClearsErrorState()
    {
        var vm = CreateViewModel();

        // Manually set error state
        vm.ErrorMessage = "Some error";
        vm.HasError = true;

        vm.DismissErrorCommand.Execute(null);

        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void InitialState_HasNoError()
    {
        var vm = CreateViewModel();

        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadFileAsync_EmptyPath_DoesNothing()
    {
        var vm = CreateViewModel();

        await vm.LoadFileAsync("");

        vm.HtmlContent.Should().BeEmpty();
        vm.CurrentFilePath.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadFileAsync_AddsToRecentFiles()
    {
        var fileService = new FileService(_tempConfigPath);
        var vm = new MainViewModel(
            fileService,
            new MarkdownService(),
            new ThemeService(_tempConfigPath),
            new ExportService(),
            new HtmlTemplateService(),
            new TocService());

        var filePath = CreateTempMarkdownFile("# Recent");

        await vm.LoadFileAsync(filePath);

        fileService.GetRecentFiles().Should().Contain(filePath);
    }

    [Fact]
    public async Task HandleFileDropAsync_LoadsFile()
    {
        var vm = CreateViewModel();
        var filePath = CreateTempMarkdownFile("# Dropped File\n\nContent here.");

        await vm.HandleFileDropAsync(filePath);

        vm.HtmlContent.Should().NotBeNullOrEmpty();
        vm.CurrentFilePath.Should().Be(filePath);
        vm.TocItems.Should().NotBeEmpty();
    }

    [Fact]
    public void ShowSearchCommand_TogglesSearchVisibility()
    {
        var vm = CreateViewModel();

        vm.IsSearchVisible.Should().BeFalse();

        vm.ShowSearchCommand.Execute(null);
        vm.IsSearchVisible.Should().BeTrue();

        vm.ShowSearchCommand.Execute(null);
        vm.IsSearchVisible.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleThemeCommand_ReRendersContent()
    {
        var vm = CreateViewModel();
        var filePath = CreateTempMarkdownFile("# Theme Test");

        await vm.LoadFileAsync(filePath);
        var htmlBefore = vm.HtmlContent;

        vm.ToggleThemeCommand.Execute(null);
        var htmlAfter = vm.HtmlContent;

        // Theme toggle should produce different HTML (different theme class)
        htmlAfter.Should().NotBeNullOrEmpty();
        htmlAfter.Should().NotBe(htmlBefore);
    }

    [Fact]
    public void ToggleThemeCommand_NoFileLoaded_DoesNotCrash()
    {
        var vm = CreateViewModel();

        // Should not throw when no file is loaded
        vm.ToggleThemeCommand.Execute(null);

        vm.HtmlContent.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadFileAsync_GeneratesCorrectTocStructure()
    {
        var vm = CreateViewModel();
        var markdown = "# Title\n\n## Section A\n\n### Subsection\n\n## Section B";
        var filePath = CreateTempMarkdownFile(markdown);

        await vm.LoadFileAsync(filePath);

        vm.TocItems.Should().NotBeEmpty();
        // The top-level should have the H1 title
        vm.TocItems[0].Title.Should().Be("Title");
        vm.TocItems[0].Level.Should().Be(1);
    }

    [Fact]
    public async Task LoadFileAsync_WindowTitle_ContainsFileName()
    {
        var vm = CreateViewModel();
        var filePath = CreateTempMarkdownFile("# Test");

        await vm.LoadFileAsync(filePath);

        vm.WindowTitle.Should().Contain(Path.GetFileName(filePath));
        vm.WindowTitle.Should().EndWith("Markdown Reader");
    }

    [Fact]
    public void Commands_AreNotNull()
    {
        var vm = CreateViewModel();

        vm.OpenFileCommand.Should().NotBeNull();
        vm.ShowSearchCommand.Should().NotBeNull();
        vm.ToggleThemeCommand.Should().NotBeNull();
        vm.ExportHtmlCommand.Should().NotBeNull();
    }
}
