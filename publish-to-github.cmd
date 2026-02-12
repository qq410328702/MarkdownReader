@echo off
REM 发布到 GitHub Release

echo ========================================
echo 发布 Markdown Reader v0.0.1 到 GitHub
echo ========================================
echo.

REM 1. 添加所有文件到 Git
echo [1/5] 添加文件到 Git...
git add .
if errorlevel 1 (
    echo Git add 失败！
    pause
    exit /b 1
)

REM 2. 提交更改
echo [2/5] 提交更改...
git commit -m "Release v0.0.1 - 首个公开发布版本"
if errorlevel 1 (
    echo Git commit 失败！可能没有更改需要提交
)

REM 3. 创建 Tag
echo [3/5] 创建 Git Tag v0.0.1...
git tag -a v0.0.1 -m "Release v0.0.1"
if errorlevel 1 (
    echo 创建 Tag 失败！Tag 可能已存在
    echo 如果 Tag 已存在，可以删除后重新创建：
    echo   git tag -d v0.0.1
    echo   git push origin :refs/tags/v0.0.1
    pause
    exit /b 1
)

REM 4. 推送代码到 GitHub
echo [4/5] 推送代码到 GitHub...
git push origin master
if errorlevel 1 (
    echo 推送代码失败！
    pause
    exit /b 1
)

REM 5. 推送 Tag 到 GitHub
echo [5/5] 推送 Tag 到 GitHub...
git push origin v0.0.1
if errorlevel 1 (
    echo 推送 Tag 失败！
    pause
    exit /b 1
)

echo.
echo ========================================
echo Git 操作完成！
echo ========================================
echo.
echo 下一步：在 GitHub 上创建 Release
echo.
echo 1. 访问: https://github.com/qq410328702/MarkdownReader/releases/new
echo 2. 选择 Tag: v0.0.1
echo 3. Release 标题: Markdown Reader v0.0.1
echo 4. 复制 Release-v0.0.1\RELEASE_NOTES.md 的内容到描述框
echo 5. 上传以下文件:
echo    - Release-v0.0.1\MarkdownReader-v0.0.1-Setup.zip
echo    - Release-v0.0.1\MarkdownReader-v0.0.1-Portable.zip
echo 6. 点击 "Publish release"
echo.
echo Release 文件位置: %cd%\Release-v0.0.1
echo.
pause
