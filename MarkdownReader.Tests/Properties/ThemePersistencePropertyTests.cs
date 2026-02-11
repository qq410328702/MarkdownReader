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
/// Property 9: 主题持久化往返一致性
/// For any 主题设置，保存主题偏好后重新加载，加载的主题应与保存时的主题一致。
/// **Validates: Requirements 7.4, 7.5**
/// </summary>
public class ThemePersistencePropertyTests : IDisposable
{
    private readonly string _tempDir;

    public ThemePersistencePropertyTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ThemePersistencePropertyTests_{Guid.NewGuid():N}");
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
    /// Generates arbitrary toggle counts (1 to 10) to produce random theme states.
    /// </summary>
    public static Arbitrary<int> ToggleCountArbitrary()
    {
        var gen = Gen.Choose(1, 10);
        return Arb.ToArbitrary(gen);
    }

    /// <summary>
    /// Property 9: Saving a theme and loading it in a new ThemeService instance yields the same theme.
    /// For any initial theme, after toggling to reach a target state and persisting,
    /// a fresh ThemeService loading from the same config file should have the same theme.
    /// **Validates: Requirements 7.4, 7.5**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ThemePersistencePropertyTests) })]
    public void SavedTheme_IsRestoredCorrectly_AfterReload(ThemeType initialTheme)
    {
        // Arrange: create a ThemeService with an isolated config file
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service1 = new ThemeService(configPath);

        // Set the initial theme by writing config and loading
        var settings = new AppSettings { Theme = initialTheme };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(configPath, json);
        service1.LoadSavedTheme();

        // Act: toggle once to change and persist the theme
        service1.ToggleTheme();
        var savedTheme = service1.CurrentTheme;

        // Assert: a new ThemeService loading from the same config should have the same theme
        var service2 = new ThemeService(configPath);
        service2.LoadSavedTheme();

        service2.CurrentTheme.Should().Be(savedTheme,
            because: "the persisted theme should be restored correctly after reload");
    }

    /// <summary>
    /// Property 9 (supplementary): After any number of toggles, the persisted theme
    /// matches the final in-memory theme when loaded in a new instance.
    /// **Validates: Requirements 7.4, 7.5**
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ThemePersistencePropertyTests) })]
    public void AfterMultipleToggles_PersistedTheme_MatchesFinalState(int toggleCount)
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, $"config_{Guid.NewGuid():N}.json");
        var service1 = new ThemeService(configPath);

        // Act: toggle N times (each toggle persists to the config file)
        for (var i = 0; i < toggleCount; i++)
        {
            service1.ToggleTheme();
        }

        var finalTheme = service1.CurrentTheme;

        // Assert: a new instance loading from the same config should match
        var service2 = new ThemeService(configPath);
        service2.LoadSavedTheme();

        service2.CurrentTheme.Should().Be(finalTheme,
            because: $"after {toggleCount} toggles, the loaded theme should match the final persisted state");
    }
}
