using System.Text.Json;
using FluentAssertions;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;

    public FileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"FileServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "appsettings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // --- ReadFileAsync Tests ---

    [Fact]
    public async Task ReadFileAsync_ReturnsFileContent()
    {
        var filePath = Path.Combine(_tempDir, "test.md");
        var content = "# Hello\n\nThis is a test.";
        await File.WriteAllTextAsync(filePath, content);

        var service = new FileService(_configPath);
        var result = await service.ReadFileAsync(filePath);

        result.Should().Be(content);
    }

    [Fact]
    public async Task ReadFileAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        var service = new FileService(_configPath);
        var act = () => service.ReadFileAsync(Path.Combine(_tempDir, "nonexistent.md"));

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReadFileAsync_ThrowsArgumentException_WhenPathIsInvalid(string? path)
    {
        var service = new FileService(_configPath);
        var act = () => service.ReadFileAsync(path!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ReadFileAsync_ReadsUtf8Content()
    {
        var filePath = Path.Combine(_tempDir, "unicode.md");
        var content = "# 你好世界\n\n数学公式 $E=mc^2$";
        await File.WriteAllTextAsync(filePath, content);

        var service = new FileService(_configPath);
        var result = await service.ReadFileAsync(filePath);

        result.Should().Be(content);
    }

    // --- AddRecentFile / GetRecentFiles Tests ---

    [Fact]
    public void GetRecentFiles_ReturnsEmptyList_WhenNoFilesAdded()
    {
        var service = new FileService(_configPath);
        service.GetRecentFiles().Should().BeEmpty();
    }

    [Fact]
    public void AddRecentFile_AddsFileToList()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\docs\\file1.md");

        service.GetRecentFiles().Should().ContainSingle()
            .Which.Should().Be("C:\\docs\\file1.md");
    }

    [Fact]
    public void AddRecentFile_MostRecentFileIsFirst()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\docs\\file1.md");
        service.AddRecentFile("C:\\docs\\file2.md");

        service.GetRecentFiles()[0].Should().Be("C:\\docs\\file2.md");
        service.GetRecentFiles()[1].Should().Be("C:\\docs\\file1.md");
    }

    [Fact]
    public void AddRecentFile_MovesDuplicateToTop()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\docs\\file1.md");
        service.AddRecentFile("C:\\docs\\file2.md");
        service.AddRecentFile("C:\\docs\\file3.md");

        // Re-add file1 — should move to top
        service.AddRecentFile("C:\\docs\\file1.md");

        var recent = service.GetRecentFiles();
        recent.Should().HaveCount(3);
        recent[0].Should().Be("C:\\docs\\file1.md");
        recent[1].Should().Be("C:\\docs\\file3.md");
        recent[2].Should().Be("C:\\docs\\file2.md");
    }

    [Fact]
    public void AddRecentFile_EnforcesMaximumOf10()
    {
        var service = new FileService(_configPath);

        for (int i = 1; i <= 12; i++)
            service.AddRecentFile($"C:\\docs\\file{i}.md");

        var recent = service.GetRecentFiles();
        recent.Should().HaveCount(10);
        recent[0].Should().Be("C:\\docs\\file12.md");
        recent.Should().NotContain("C:\\docs\\file1.md");
        recent.Should().NotContain("C:\\docs\\file2.md");
    }

    [Fact]
    public void AddRecentFile_IgnoresNullOrWhitespace()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("");
        service.AddRecentFile("   ");

        service.GetRecentFiles().Should().BeEmpty();
    }

    [Fact]
    public void AddRecentFile_DuplicateCheckIsCaseInsensitive()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\Docs\\File.md");
        service.AddRecentFile("c:\\docs\\file.md");

        service.GetRecentFiles().Should().HaveCount(1);
        service.GetRecentFiles()[0].Should().Be("c:\\docs\\file.md");
    }

    // --- Persistence Tests ---

    [Fact]
    public void AddRecentFile_PersistsToConfigFile()
    {
        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\docs\\file1.md");

        File.Exists(_configPath).Should().BeTrue();
        var json = File.ReadAllText(_configPath);
        json.Should().Contain("file1.md");
    }

    [Fact]
    public void Constructor_LoadsRecentFilesFromConfig()
    {
        // Arrange: pre-populate config
        var settings = new AppSettings
        {
            RecentFiles = new List<string> { "C:\\a.md", "C:\\b.md" }
        };
        File.WriteAllText(_configPath, JsonSerializer.Serialize(settings));

        // Act
        var service = new FileService(_configPath);

        // Assert
        service.GetRecentFiles().Should().BeEquivalentTo(
            new[] { "C:\\a.md", "C:\\b.md" },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void AddRecentFile_PreservesThemeInConfig()
    {
        // Pre-populate config with a theme setting
        var settings = new AppSettings
        {
            Theme = ThemeType.Dark,
            RecentFiles = new List<string>()
        };
        File.WriteAllText(_configPath, JsonSerializer.Serialize(settings));

        var service = new FileService(_configPath);
        service.AddRecentFile("C:\\docs\\new.md");

        // Verify theme is preserved
        var json = File.ReadAllText(_configPath);
        var updated = JsonSerializer.Deserialize<AppSettings>(json);
        updated!.Theme.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void Constructor_HandlesCorruptConfig()
    {
        File.WriteAllText(_configPath, "not valid json!!!");

        var service = new FileService(_configPath);
        service.GetRecentFiles().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_HandlesMissingConfig()
    {
        var service = new FileService(_configPath);
        service.GetRecentFiles().Should().BeEmpty();
    }
}
