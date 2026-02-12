# 创建 ICO 图标文件的 PowerShell 脚本
# 需要安装 ImageMagick 或使用在线转换工具

Write-Host "Markdown Reader 图标生成工具" -ForegroundColor Green
Write-Host "================================`n" -ForegroundColor Green

$svgFile = ".\MarkdownReader\app-icon.svg"
$icoFile = ".\MarkdownReader\app.ico"

# 检查 SVG 文件是否存在
if (-not (Test-Path $svgFile)) {
    Write-Host "错误: 找不到 SVG 文件: $svgFile" -ForegroundColor Red
    exit 1
}

Write-Host "SVG 文件已找到: $svgFile`n" -ForegroundColor Yellow

# 方法 1: 检查是否安装了 ImageMagick
Write-Host "检查 ImageMagick..." -ForegroundColor Yellow
$magickPath = Get-Command magick -ErrorAction SilentlyContinue

if ($magickPath) {
    Write-Host "找到 ImageMagick，开始转换..." -ForegroundColor Green
    
    # 使用 ImageMagick 转换 SVG 到 ICO (多种尺寸)
    & magick convert $svgFile `
        -define icon:auto-resize=256,128,64,48,32,16 `
        $icoFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ 图标创建成功: $icoFile" -ForegroundColor Green
        Write-Host "  包含尺寸: 256x256, 128x128, 64x64, 48x48, 32x32, 16x16" -ForegroundColor Cyan
        exit 0
    } else {
        Write-Host "转换失败!" -ForegroundColor Red
    }
} else {
    Write-Host "未找到 ImageMagick`n" -ForegroundColor Yellow
}

# 方法 2: 提供替代方案
Write-Host "=" * 60 -ForegroundColor Gray
Write-Host "`n替代方案：" -ForegroundColor Yellow
Write-Host "`n1. 安装 ImageMagick (推荐)" -ForegroundColor Cyan
Write-Host "   下载地址: https://imagemagick.org/script/download.php#windows"
Write-Host "   安装后重新运行此脚本"

Write-Host "`n2. 使用在线转换工具" -ForegroundColor Cyan
Write-Host "   a) 打开浏览器访问以下任一网站:"
Write-Host "      - https://convertio.co/zh/svg-ico/"
Write-Host "      - https://cloudconvert.com/svg-to-ico"
Write-Host "      - https://www.aconvert.com/icon/svg-to-ico/"
Write-Host "   b) 上传文件: $svgFile"
Write-Host "   c) 选择输出格式: ICO"
Write-Host "   d) 下载转换后的文件并保存为: $icoFile"

Write-Host "`n3. 使用 Windows 内置工具" -ForegroundColor Cyan
Write-Host "   a) 在浏览器中打开 SVG 文件"
Write-Host "   b) 截图或另存为 PNG"
Write-Host "   c) 使用 Paint 或其他工具调整大小为 256x256"
Write-Host "   d) 使用在线工具转换 PNG 到 ICO"

Write-Host "`n4. 使用 Inkscape (免费开源)" -ForegroundColor Cyan
Write-Host "   下载地址: https://inkscape.org/release/"
Write-Host "   打开 SVG 文件，导出为 PNG，然后转换为 ICO"

Write-Host "`n" + ("=" * 60) -ForegroundColor Gray

# 尝试在浏览器中打开 SVG 预览
Write-Host "`n是否在浏览器中预览 SVG 图标? (Y/N): " -ForegroundColor Yellow -NoNewline
$response = Read-Host

if ($response -eq 'Y' -or $response -eq 'y') {
    Start-Process $svgFile
    Write-Host "已在浏览器中打开 SVG 文件" -ForegroundColor Green
}

Write-Host "`n提示: 创建 ICO 文件后，重新运行构建脚本即可使用新图标" -ForegroundColor Cyan
