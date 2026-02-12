# Markdown Reader 安装包构建脚本

Write-Host "开始构建 Markdown Reader 安装包..." -ForegroundColor Green

# 1. 清理之前的构建
Write-Host "`n清理之前的构建..." -ForegroundColor Yellow
if (Test-Path ".\MarkdownReader\bin\Release") {
    Remove-Item ".\MarkdownReader\bin\Release" -Recurse -Force
}
if (Test-Path ".\Setup\bin") {
    Remove-Item ".\Setup\bin" -Recurse -Force
}
if (Test-Path ".\Setup\obj") {
    Remove-Item ".\Setup\obj" -Recurse -Force
}

# 2. 发布应用程序
Write-Host "`n发布应用程序..." -ForegroundColor Yellow
dotnet publish .\MarkdownReader\MarkdownReader.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "发布失败!" -ForegroundColor Red
    exit 1
}

Write-Host "发布成功!" -ForegroundColor Green

# 3. 检查 WiX Toolset
Write-Host "`n检查 WiX Toolset..." -ForegroundColor Yellow
$wixPath = "${env:WIX}bin\candle.exe"
if (-not (Test-Path $wixPath)) {
    Write-Host "错误: 未找到 WiX Toolset!" -ForegroundColor Red
    Write-Host "请从以下地址下载并安装 WiX Toolset v3.11 或更高版本:" -ForegroundColor Yellow
    Write-Host "https://wixtoolset.org/releases/" -ForegroundColor Cyan
    exit 1
}

# 4. 构建 WiX 安装包
Write-Host "`n构建安装包..." -ForegroundColor Yellow
$publishDir = Resolve-Path ".\MarkdownReader\bin\Release\net8.0-windows\win-x64\publish"

# 编译 .wxs 文件
& "${env:WIX}bin\candle.exe" `
    -dPublishDir="$publishDir" `
    -out ".\Setup\obj\" `
    ".\Setup\Product.wxs" `
    ".\Setup\Files.wxs"

if ($LASTEXITCODE -ne 0) {
    Write-Host "编译 WiX 文件失败!" -ForegroundColor Red
    exit 1
}

# 链接生成 .msi
New-Item -ItemType Directory -Force -Path ".\Setup\bin" | Out-Null
& "${env:WIX}bin\light.exe" `
    -out ".\Setup\bin\MarkdownReaderSetup.msi" `
    -ext WixUIExtension `
    ".\Setup\obj\Product.wixobj" `
    ".\Setup\obj\Files.wixobj"

if ($LASTEXITCODE -ne 0) {
    Write-Host "生成安装包失败!" -ForegroundColor Red
    exit 1
}

Write-Host "`n构建完成!" -ForegroundColor Green
Write-Host "安装包位置: .\Setup\bin\MarkdownReaderSetup.msi" -ForegroundColor Cyan
Write-Host "`n安装包功能:" -ForegroundColor Yellow
Write-Host "  ✓ 自动关联 .md, .markdown, .mdown 文件" -ForegroundColor White
Write-Host "  ✓ 创建开始菜单快捷方式" -ForegroundColor White
Write-Host "  ✓ 创建桌面快捷方式" -ForegroundColor White
Write-Host "  ✓ 支持完整卸载" -ForegroundColor White
