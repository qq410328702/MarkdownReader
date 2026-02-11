using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 5: 最近文件列表不变量
/// For any 文件打开操作序列，最近文件列表的长度不应超过 10 条，
/// 且最近打开的文件应位于列表首位。重复打开同一文件应将其移至列表首位而非产生重复条目。
/// **Validates: Requirements 4.4**
/// </summary>
public class RecentFilesPropertyTests : IDisposable
{
    private readonly string _tempDir;

    public RecentFilesPropertyTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"RecentFilesPropertyTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    /// <summary>
    /// Generates arbitrary sequences of file paths (1 to 20 paths per sequence).
    /// Paths are drawn from a pool to ensure some duplicates occur naturally.
    /// </summary>
    public static Arbitrary<List<string>> FilePathSequenceArbitrary()
    {
        var fileNameGen = Gen.Elements(
            @"C:\docs\readme.md",
            @"C:\docs\guide.md",
            @"C:\docs\notes.md",
            @"C:\projects\spec.md",
            @"C:\projects\design.md",
            @"C:\projects\tasks.md",
            @"C:\temp\draft.md",
            @"C:\temp\scratch.md",
            @"C:\reports\summary.md",
            @"C:\reports\analysis.md",
            @"C:\reports\review.md",
            @"C:\data\input.md",
            @"C:\data\output.md",
            @"C:\archive\old.md",
            @"C:\archive\backup.md");

        var sequenceGen = Gen.Choose(1, 20).SelectMany(count =>
            Gen.ListOf(fileNameGen, count)
                .Select(items => items.ToList()));

        return Arb.ToArbitrary(sequenceGen);
    }

    /// <summary>
    /// Property 5: For any sequence of file open operations, the recent files list
    /// length never exceeds 10 items.
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(RecentFilesPropertyTests) })]
    public void RecentFilesList_NeverExceeds_MaximumOf10(List<string> filePaths)
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service = new FileService(configPath);

        // Act & Assert: after each addition, the list should never exceed 10
        foreach (var path in filePaths)
        {
            service.AddRecentFile(path);
            service.GetRecentFiles().Count.Should().BeLessThanOrEqualTo(10,
                because: "the recent files list must never exceed 10 items");
        }
    }

    /// <summary>
    /// Property 5: For any file path added, it should always appear at position 0
    /// (the most recently added file is at the top).
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(RecentFilesPropertyTests) })]
    public void MostRecentlyAddedFile_IsAlways_AtPositionZero(List<string> filePaths)
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service = new FileService(configPath);

        // Act & Assert: after each addition, the added file should be first
        foreach (var path in filePaths)
        {
            service.AddRecentFile(path);
            service.GetRecentFiles()[0].Should().Be(path,
                because: "the most recently added file should always be at position 0");
        }
    }

    /// <summary>
    /// Property 5: Adding a duplicate file moves it to the top without creating
    /// duplicate entries in the list.
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(RecentFilesPropertyTests) })]
    public void DuplicateFile_MovesToTop_WithoutCreatingDuplicates(List<string> filePaths)
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service = new FileService(configPath);

        // Act: add all files
        foreach (var path in filePaths)
        {
            service.AddRecentFile(path);
        }

        // Assert: no duplicates in the final list (case-insensitive)
        var recentFiles = service.GetRecentFiles();
        var distinctCount = recentFiles
            .Select(f => f.ToUpperInvariant())
            .Distinct()
            .Count();

        distinctCount.Should().Be(recentFiles.Count,
            because: "the recent files list should never contain duplicate entries");
    }
}
