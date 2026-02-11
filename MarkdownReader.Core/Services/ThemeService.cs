using System.IO;
using System.Text.Json;
using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public class ThemeService : IThemeService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _configPath;

    public ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

    public event EventHandler<ThemeType>? ThemeChanged;

    public ThemeService(string? configPath = null)
    {
        _configPath = configPath
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    }

    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeType.Light
            ? ThemeType.Dark
            : ThemeType.Light;

        ThemeChanged?.Invoke(this, CurrentTheme);
        SaveSettings();
    }

    public void LoadSavedTheme()
    {
        try
        {
            if (!File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

            if (settings is not null)
            {
                CurrentTheme = settings.Theme;
                ThemeChanged?.Invoke(this, CurrentTheme);
            }
        }
        catch
        {
            // If config is corrupt or unreadable, keep default Light theme
        }
    }

    private void SaveSettings()
    {
        try
        {
            AppSettings settings;

            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                settings = new AppSettings();
            }

            settings.Theme = CurrentTheme;

            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var updatedJson = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_configPath, updatedJson);
        }
        catch
        {
            // Silently fail on save errors — don't crash the app
        }
    }
}
