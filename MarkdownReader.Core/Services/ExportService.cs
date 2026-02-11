using System.IO;
using Microsoft.Web.WebView2.Wpf;

namespace MarkdownReader.Core.Services;

public class ExportService : IExportService
{
    /// <summary>
    /// 导出为独立 HTML 文件。将完整的 HTML 内容写入指定路径。
    /// </summary>
    public async Task ExportToHtmlAsync(string htmlContent, string filePath)
    {
        if (string.IsNullOrEmpty(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(filePath, htmlContent);
    }

    /// <summary>
    /// 通过 WebView2 的 PrintToPdfAsync 导出为 PDF 文件。
    /// </summary>
    public async Task ExportToPdfAsync(WebView2 webView, string filePath)
    {
        if (webView is null)
            throw new ArgumentNullException(nameof(webView));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (webView.CoreWebView2 is null)
            throw new InvalidOperationException("WebView2 is not initialized. Ensure CoreWebView2 is ready before exporting to PDF.");

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await webView.CoreWebView2.PrintToPdfAsync(filePath);
    }
}
