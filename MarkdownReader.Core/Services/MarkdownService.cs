using Markdig;

namespace MarkdownReader.Core.Services;

public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()    // GFM 表格、任务列表、删除线等
            .UseMathematics()           // LaTeX 数学公式 $...$ 和 $$...$$
            .Build();
    }

    public string ConvertToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml(markdown, _pipeline);
    }

    public string ConvertToPlainText(string markdown)
    {
        return Markdig.Markdown.ToPlainText(markdown, _pipeline);
    }
}
