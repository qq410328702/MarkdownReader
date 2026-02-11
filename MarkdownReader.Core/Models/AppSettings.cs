namespace MarkdownReader.Core.Models;

public class AppSettings
{
    /// <summary>当前主题</summary>
    public ThemeType Theme { get; set; } = ThemeType.Light;

    /// <summary>最近打开的文件列表</summary>
    public List<string> RecentFiles { get; set; } = new();
}
