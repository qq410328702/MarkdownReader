using FluentAssertions;
using MarkdownReader.Core.Models;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests.Services;

public class ThemeServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;

    public ThemeServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ThemeServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "appsettings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void CurrentTheme_DefaultsToLight()
    {
        var service = new ThemeService(_configPath);
        service.CurrentTheme.Should().Be(ThemeType.Light);
    }

    [Fact]
    public void ToggleTheme_SwitchesFromLightToDark()
    {
        var service = new ThemeService(_configPath);
        service.ToggleTheme();
        service.CurrentTheme.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void ToggleTheme_SwitchesFromDarkToLight()
    {
        var service = new ThemeService(_configPath);
        service.ToggleTheme(); // Light -> Dark
        service.ToggleTheme(); // Dark -> Light
        service.CurrentTheme.Should().Be(ThemeType.Light);
    }

    [Fact]
    public void ToggleTheme_FiresThemeChangedEvent()
    {
        var service = new ThemeService(_configPath);
        ThemeType? received = null;
        service.ThemeChanged += (_, theme) => received = theme;

        service.ToggleTheme();

        received.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void ToggleTheme_SavesSettingsToConfigFile()
    {
        var service = new ThemeService(_configPath);
        service.ToggleTheme();

        File.Exists(_configPath).Should().BeTrue();
        var json = File.ReadAllText(_configPath);
        json.Should().Contain("\"Theme\"");
    }

    [Fact]
    public void LoadSavedTheme_LoadsDarkThemeFromConfig()
    {
        // Arrange: save a Dark theme config
        var settings = new AppSettings { Theme = ThemeType.Dark };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(_configPath, json);

        // Act
        var service = new ThemeService(_configPath);
        service.LoadSavedTheme();

        // Assert
        service.CurrentTheme.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void LoadSavedTheme_FiresThemeChangedEvent()
    {
        var settings = new AppSettings { Theme = ThemeType.Dark };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(_configPath, json);

        var service = new ThemeService(_configPath);
        ThemeType? received = null;
        service.ThemeChanged += (_, theme) => received = theme;

        service.LoadSavedTheme();

        received.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void LoadSavedTheme_KeepsLightWhenNoConfigFile()
    {
        var service = new ThemeService(_configPath);
        service.LoadSavedTheme();
        service.CurrentTheme.Should().Be(ThemeType.Light);
    }

    [Fact]
    public void LoadSavedTheme_KeepsLightWhenConfigIsCorrupt()
    {
        File.WriteAllText(_configPath, "not valid json!!!");

        var service = new ThemeService(_configPath);
        service.LoadSavedTheme();

        service.CurrentTheme.Should().Be(ThemeType.Light);
    }

    [Fact]
    public void ToggleTheme_PersistenceRoundTrip()
    {
        // Toggle to Dark and save
        var service1 = new ThemeService(_configPath);
        service1.ToggleTheme();

        // Load in a new instance
        var service2 = new ThemeService(_configPath);
        service2.LoadSavedTheme();

        service2.CurrentTheme.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void ToggleTheme_PreservesRecentFilesInConfig()
    {
        // Pre-populate config with recent files
        var settings = new AppSettings
        {
            Theme = ThemeType.Light,
            RecentFiles = new List<string> { "file1.md", "file2.md" }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(_configPath, json);

        // Toggle theme
        var service = new ThemeService(_configPath);
        service.LoadSavedTheme();
        service.ToggleTheme();

        // Verify recent files are preserved
        var updatedJson = File.ReadAllText(_configPath);
        var updatedSettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(updatedJson);
        updatedSettings!.RecentFiles.Should().BeEquivalentTo(new[] { "file1.md", "file2.md" });
    }
}
