using Microsoft.Web.WebView2.Wpf;

namespace MarkdownReader.Core.Services;

public interface IExportService
{
    /// <summary>导出为独立 HTML 文件</summary>
    Task ExportToHtmlAsync(string htmlContent, string filePath);

    /// <summary>通过 WebView2 导出为 PDF</summary>
    Task ExportToPdfAsync(WebView2 webView, string filePath);
}
