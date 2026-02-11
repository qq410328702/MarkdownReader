using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;
using Gen = FsCheck.Fluent.Gen;
using Arb = FsCheck.Fluent.Arb;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 8: 主题切换对称性
/// For any 初始主题状态，执行两次 ToggleTheme 操作后，主题应恢复为初始状态
/// （Light → Dark → Light，Dark → Light → Dark）。
/// **Validates: Requirements 7.2**
/// </summary>
public class ThemeTogglePropertyTests : IDisposable
{
    private readonly string _tempDir;

    public ThemeTogglePropertyTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ThemeTogglePropertyTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    /// <summary>
    /// Generates arbitrary ThemeType values (Light or Dark).
    /// </summary>
    public static Arbitrary<ThemeType> ThemeTypeArbitrary()
    {
        var gen = Gen.Elements(ThemeType.Light, ThemeType.Dark);
        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Property 8: Toggling the theme twice returns to the original state.
    /// For any initial theme, ToggleTheme() called twice yields the same theme.
    /// **Validates: Requirements 7.2**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ThemeTogglePropertyTests) })]
    public void ToggleThemeTwice_ReturnsToOriginalState(ThemeType initialTheme)
    {
        // Arrange: create a ThemeService with an isolated config file
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service = new ThemeService(configPath);

        // Set the initial theme by writing a config and loading it
        var settings = new AppSettings { Theme = initialTheme };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(configPath, json);
        service.LoadSavedTheme();

        service.CurrentTheme.Should().Be(initialTheme,
            because: "the service should start with the loaded initial theme");

        // Act: toggle twice
        service.ToggleTheme();
        service.ToggleTheme();

        // Assert: theme should be back to the initial state
        service.CurrentTheme.Should().Be(initialTheme,
            because: $"toggling theme twice from {initialTheme} should return to {initialTheme}");
    }
}
