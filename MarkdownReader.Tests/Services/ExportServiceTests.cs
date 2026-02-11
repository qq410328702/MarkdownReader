using FluentAssertions;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests.Services;

public class ExportServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ExportServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _service = new ExportService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task ExportToHtmlAsync_WritesCompleteContentToFile()
    {
        var htmlContent = "<html><body><h1>Hello</h1></body></html>";
        var filePath = Path.Combine(_tempDir, "output.html");

        await _service.ExportToHtmlAsync(htmlContent, filePath);

        var written = await File.ReadAllTextAsync(filePath);
        written.Should().Be(htmlContent);
    }

    [Fact]
    public async Task ExportToHtmlAsync_WritesStandaloneHtmlFile()
    {
        var htmlContent = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head><body><p>Test</p></body></html>";
        var filePath = Path.Combine(_tempDir, "standalone.html");

        await _service.ExportToHtmlAsync(htmlContent, filePath);

        var written = await File.ReadAllTextAsync(filePath);
        written.Should().Contain("<!DOCTYPE html>");
        written.Should().Contain("<html>");
        written.Should().Contain("</html>");
    }

    [Fact]
    public async Task ExportToHtmlAsync_CreatesDirectoryIfNotExists()
    {
        var nestedDir = Path.Combine(_tempDir, "sub", "folder");
        var filePath = Path.Combine(nestedDir, "output.html");

        await _service.ExportToHtmlAsync("<html></html>", filePath);

        Directory.Exists(nestedDir).Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExportToHtmlAsync_ThrowsArgumentException_ForNullOrEmptyHtmlContent(string? htmlContent)
    {
        var filePath = Path.Combine(_tempDir, "output.html");

        var act = () => _service.ExportToHtmlAsync(htmlContent!, filePath);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("htmlContent");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExportToHtmlAsync_ThrowsArgumentException_ForNullOrEmptyFilePath(string? filePath)
    {
        var act = () => _service.ExportToHtmlAsync("<html></html>", filePath!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }
}
