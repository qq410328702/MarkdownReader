using FluentAssertions;
using FsCheck.Xunit;
using MarkdownReader.Core.Services;

namespace MarkdownReader.Tests.Properties;

/// <summary>
/// Property 3: 解析器错误容错性
/// For any 任意字符串输入（包括无效 Markdown 语法），Markdown_Parser 的 ConvertToHtml 方法
/// 不应抛出异常，而应返回有效的 HTML 字符串。
/// **Validates: Requirements 1.7**
/// </summary>
public class ParserErrorTolerancePropertyTests
{
    private readonly MarkdownService _sut = new();

    /// <summary>
    /// Property 3: ConvertToHtml never throws and always returns a non-null string
    /// for any arbitrary string input (including null, empty, and random characters).
    /// **Validates: Requirements 1.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public void ConvertToHtml_NeverThrows_ForAnyInput(string? input)
    {
        // Act
        var act = () => _sut.ConvertToHtml(input!);

        // Assert: should not throw any exception
        var result = act.Should().NotThrow(
            because: "the parser should handle any input without crashing, including invalid Markdown syntax")
            .Subject;

        // Assert: result should be a non-null string
        result.Should().NotBeNull(
            because: "ConvertToHtml should always return a valid string, never null");
    }

    /// <summary>
    /// Property 3 (supplementary): ConvertToPlainText never throws and always returns
    /// a non-null string for any arbitrary string input.
    /// **Validates: Requirements 1.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public void ConvertToPlainText_NeverThrows_ForAnyInput(string? input)
    {
        // Act
        var act = () => _sut.ConvertToPlainText(input!);

        // Assert: should not throw any exception
        var result = act.Should().NotThrow(
            because: "the parser should handle any input without crashing, including invalid Markdown syntax")
            .Subject;

        // Assert: result should be a non-null string
        result.Should().NotBeNull(
            because: "ConvertToPlainText should always return a valid string, never null");
    }
}
