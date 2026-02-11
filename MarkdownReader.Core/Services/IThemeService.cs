using MarkdownReader.Core.Models;

namespace MarkdownReader.Core.Services;

public interface IThemeService
{
    /// <summary>当前主题</summary>
    ThemeType CurrentTheme { get; }

    /// <summary>切换主题（亮色/暗色）</summary>
    void ToggleTheme();

    /// <summary>加载保存的主题设置</summary>
    void LoadSavedTheme();

    /// <summary>主题变更事件</summary>
    event EventHandler<ThemeType>? ThemeChanged;
}
