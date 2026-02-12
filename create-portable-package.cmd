@echo off
REM 创建便携版安装包

echo ========================================
echo 创建 Markdown Reader 便携版安装包
echo ========================================
echo.

REM 检查发布目录
if not exist "MarkdownReader\bin\Release\net8.0-windows\win-x64\publish" (
    echo 错误: 找不到发布目录！
    echo 请先运行发布命令。
    pause
    exit /b 1
)

REM 创建打包目录
set PACKAGE_DIR=MarkdownReader-Portable
if exist "%PACKAGE_DIR%" rd /s /q "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%\publish"

echo 复制程序文件...
xcopy /E /I /Y "MarkdownReader\bin\Release\net8.0-windows\win-x64\publish\*" "%PACKAGE_DIR%\publish\"

echo 复制安装脚本...
copy install.cmd "%PACKAGE_DIR%\"

echo 创建说明文件...
(
echo Markdown Reader 便携版安装包
echo ================================
echo.
echo 安装方法:
echo 1. 右键点击 install.cmd
echo 2. 选择"以管理员身份运行"
echo 3. 按照提示完成安装
echo.
echo 功能:
echo - 自动关联 .md, .markdown, .mdown 文件
echo - 创建开始菜单快捷方式
echo - 创建桌面快捷方式
echo - 可通过控制面板卸载
echo.
echo 卸载方法:
echo - 通过 Windows 设置 ^> 应用 ^> 已安装的应用 ^> Markdown Reader ^> 卸载
echo.
echo 版本: 1.0.0
) > "%PACKAGE_DIR%\README.txt"

echo.
echo 打包完成！
echo.
echo 安装包位置: %PACKAGE_DIR%
echo.
echo 你可以将整个 %PACKAGE_DIR% 文件夹压缩成 ZIP 文件分发。
echo 用户解压后，以管理员身份运行 install.cmd 即可安装。
echo.
pause
