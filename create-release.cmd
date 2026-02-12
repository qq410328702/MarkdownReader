@echo off
REM 创建 GitHub Release 发布包

echo ========================================
echo 创建 Markdown Reader v0.0.1 Release
echo ========================================
echo.

REM 检查便携版安装包
if not exist "MarkdownReader-Portable" (
    echo 错误: 找不到 MarkdownReader-Portable 目录！
    echo 请先运行 create-portable-package.cmd
    pause
    exit /b 1
)

REM 创建 Release 目录
set RELEASE_DIR=Release-v0.0.1
if exist "%RELEASE_DIR%" rd /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

echo 正在打包安装包...

REM 压缩便携版安装包
powershell -Command "Compress-Archive -Path 'MarkdownReader-Portable\*' -DestinationPath '%RELEASE_DIR%\MarkdownReader-v0.0.1-Setup.zip' -Force"

if errorlevel 1 (
    echo 压缩失败！
    pause
    exit /b 1
)

echo 压缩完成！
echo.

REM 创建绿色版（不需要安装）
echo 正在创建绿色版...
mkdir "%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable"
xcopy /E /I /Y "MarkdownReader\bin\Release\net8.0-windows\win-x64\publish\*" "%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable\"

REM 添加绿色版说明
(
echo Markdown Reader v0.0.1 - 绿色便携版
echo =====================================
echo.
echo 使用方法:
echo 1. 直接运行 MarkdownReader.exe
echo 2. 将 .md 文件拖放到程序窗口打开
echo.
echo 注意:
echo - 绿色版不会自动关联文件
echo - 如需文件关联，请使用安装版
echo.
echo 版本: 0.0.1
) > "%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable\README.txt"

REM 压缩绿色版
powershell -Command "Compress-Archive -Path '%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable' -DestinationPath '%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable.zip' -Force"

REM 删除临时文件夹
rd /s /q "%RELEASE_DIR%\MarkdownReader-v0.0.1-Portable"

echo 绿色版创建完成！
echo.

REM 创建 Release Notes
(
echo # Markdown Reader v0.0.1
echo.
echo 首个公开发布版本！
echo.
echo ## 功能特性
echo.
echo - 📖 支持 Markdown 文件阅读和预览
echo - 🎨 现代化的用户界面
echo - 🔍 内置搜索功能
echo - 📑 自动生成目录
echo - 🌓 支持主题切换
echo - 📤 支持导出为 HTML
echo - 🔗 自动关联 .md, .markdown, .mdown 文件
echo.
echo ## 下载说明
echo.
echo ### 安装版 ^(推荐^)
echo.
echo **文件**: `MarkdownReader-v0.0.1-Setup.zip`
echo.
echo - 自动关联 Markdown 文件
echo - 创建开始菜单和桌面快捷方式
echo - 支持通过控制面板卸载
echo.
echo **安装步骤**:
echo 1. 下载并解压 `MarkdownReader-v0.0.1-Setup.zip`
echo 2. 右键点击 `install.cmd`
echo 3. 选择"以管理员身份运行"
echo 4. 按照提示完成安装
echo.
echo ### 绿色便携版
echo.
echo **文件**: `MarkdownReader-v0.0.1-Portable.zip`
echo.
echo - 无需安装，解压即用
echo - 不修改系统设置
echo - 适合临时使用或 U 盘携带
echo.
echo **使用方法**:
echo 1. 下载并解压 `MarkdownReader-v0.0.1-Portable.zip`
echo 2. 运行 `MarkdownReader.exe`
echo 3. 拖放 .md 文件到窗口打开
echo.
echo ## 系统要求
echo.
echo - Windows 10 或更高版本
echo - .NET 8.0 运行时 ^(已包含^)
echo - 约 150 MB 磁盘空间
echo.
echo ## 已知问题
echo.
echo - 首次启动可能需要几秒钟
echo - 某些杀毒软件可能误报，请添加信任
echo.
echo ## 反馈与支持
echo.
echo 如有问题或建议，请在 Issues 中反馈。
echo.
echo ---
echo.
echo **完整更新日志**: 首个发布版本
) > "%RELEASE_DIR%\RELEASE_NOTES.md"

echo.
echo ========================================
echo Release 包创建完成！
echo ========================================
echo.
echo 位置: %RELEASE_DIR%
echo.
echo 包含文件:
echo   - MarkdownReader-v0.0.1-Setup.zip      (安装版)
echo   - MarkdownReader-v0.0.1-Portable.zip   (绿色版)
echo   - RELEASE_NOTES.md                     (发布说明)
echo.
echo 下一步:
echo 1. 检查 Release 包内容
echo 2. 提交代码到 Git
echo 3. 创建 Git Tag: git tag v0.0.1
echo 4. 推送到 GitHub: git push origin v0.0.1
echo 5. 在 GitHub 上创建 Release 并上传文件
echo.
pause
