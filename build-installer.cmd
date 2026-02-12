@echo off
REM Markdown Reader 安装包构建脚本 (CMD 版本)

echo 开始构建 Markdown Reader 安装包...
echo.

REM 1. 清理之前的构建
echo 清理之前的构建...
if exist "MarkdownReader\bin\Release" rmdir /s /q "MarkdownReader\bin\Release"
if exist "Setup\bin" rmdir /s /q "Setup\bin"
if exist "Setup\obj" rmdir /s /q "Setup\obj"

REM 2. 发布应用程序
echo.
echo 发布应用程序...
dotnet publish MarkdownReader\MarkdownReader.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishReadyToRun=true

if errorlevel 1 (
    echo 发布失败!
    exit /b 1
)

echo 发布成功!

REM 3. 检查 WiX Toolset
echo.
echo 检查 WiX Toolset...
if not exist "%WIX%bin\candle.exe" (
    echo 错误: 未找到 WiX Toolset!
    echo 请从以下地址下载并安装 WiX Toolset v3.11 或更高版本:
    echo https://wixtoolset.org/releases/
    exit /b 1
)

REM 4. 构建 WiX 安装包
echo.
echo 构建安装包...
set PUBLISH_DIR=%CD%\MarkdownReader\bin\Release\net8.0-windows\win-x64\publish

REM 编译 .wxs 文件
if not exist "Setup\obj" mkdir "Setup\obj"
"%WIX%bin\candle.exe" -dPublishDir="%PUBLISH_DIR%" -out "Setup\obj\\" "Setup\Product.wxs" "Setup\Files.wxs"

if errorlevel 1 (
    echo 编译 WiX 文件失败!
    exit /b 1
)

REM 链接生成 .msi
if not exist "Setup\bin" mkdir "Setup\bin"
"%WIX%bin\light.exe" -out "Setup\bin\MarkdownReaderSetup.msi" -ext WixUIExtension "Setup\obj\Product.wixobj" "Setup\obj\Files.wixobj"

if errorlevel 1 (
    echo 生成安装包失败!
    exit /b 1
)

echo.
echo 构建完成!
echo 安装包位置: Setup\bin\MarkdownReaderSetup.msi
echo.
echo 安装包功能:
echo   * 自动关联 .md, .markdown, .mdown 文件
echo   * 创建开始菜单快捷方式
echo   * 创建桌面快捷方式
echo   * 支持完整卸载
