namespace MarkdownReader.Core.Services;

public interface IFileService
{
    /// <summary>读取文件内容</summary>
    Task<string> ReadFileAsync(string filePath);

    /// <summary>获取最近文件列表</summary>
    IReadOnlyList<string> GetRecentFiles();

    /// <summary>添加文件到最近列表（最多 10 条）</summary>
    void AddRecentFile(string filePath);

    /// <summary>显示文件打开对话框，返回选中的文件路径，取消返回 null</summary>
    string? ShowOpenFileDialog();

    /// <summary>显示文件保存对话框</summary>
    string? ShowSaveFileDialog(string filter, string defaultExtension);
}
