using System.IO;
using System.Text.Json;
using MarkdownReader.Core.Models;
using Microsoft.Win32;

namespace MarkdownReader.Core.Services;

public class FileService : IFileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private const int MaxRecentFiles = 10;

    private readonly string _configPath;
    private readonly List<string> _recentFiles = new();

    public FileService(string? configPath = null)
    {
        _configPath = configPath
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        LoadRecentFiles();
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file was not found.", filePath);

        return await File.ReadAllTextAsync(filePath);
    }

    public IReadOnlyList<string> GetRecentFiles()
    {
        return _recentFiles.AsReadOnly();
    }

    public void AddRecentFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // Remove duplicate if exists (case-insensitive on Windows)
        _recentFiles.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));

        // Insert at the top
        _recentFiles.Insert(0, filePath);

        // Trim to max
        while (_recentFiles.Count > MaxRecentFiles)
            _recentFiles.RemoveAt(_recentFiles.Count - 1);

        SaveRecentFiles();
    }

    public string? ShowOpenFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Markdown Files (*.md;*.markdown)|*.md;*.markdown|All Files (*.*)|*.*",
            Title = "打开 Markdown 文件"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string defaultExtension)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExtension,
            Title = "保存文件"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void LoadRecentFiles()
    {
        try
        {
            if (!File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

            if (settings?.RecentFiles is not null)
            {
                _recentFiles.Clear();
                _recentFiles.AddRange(settings.RecentFiles);
            }
        }
        catch
        {
            // If config is corrupt or unreadable, start with empty list
        }
    }

    private void SaveRecentFiles()
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

            settings.RecentFiles = new List<string>(_recentFiles);

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
