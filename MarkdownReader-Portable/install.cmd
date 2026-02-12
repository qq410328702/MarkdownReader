@echo off
REM Markdown Reader 安装脚本
REM 此脚本会将程序复制到 Program Files 并设置文件关联

echo ========================================
echo Markdown Reader 安装程序
echo ========================================
echo.

REM 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 错误: 需要管理员权限！
    echo 请右键点击此文件，选择"以管理员身份运行"
    echo.
    pause
    exit /b 1
)

echo 正在安装 Markdown Reader...
echo.

REM 设置安装目录
set INSTALL_DIR=%ProgramFiles%\MarkdownReader
set CURRENT_DIR=%~dp0

REM 创建安装目录
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM 复制文件
echo 复制程序文件...
xcopy /E /I /Y "%CURRENT_DIR%publish\*" "%INSTALL_DIR%\"

if errorlevel 1 (
    echo 文件复制失败！
    pause
    exit /b 1
)

echo 文件复制完成！
echo.

REM 注册文件关联
echo 注册文件关联...

REM 注册 .md 文件
reg add "HKEY_CLASSES_ROOT\.md" /ve /d "MarkdownReader.Document" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\.md" /v "Content Type" /d "text/markdown" /f >nul 2>&1

REM 注册 .markdown 文件
reg add "HKEY_CLASSES_ROOT\.markdown" /ve /d "MarkdownReader.Document" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\.markdown" /v "Content Type" /d "text/markdown" /f >nul 2>&1

REM 注册 .mdown 文件
reg add "HKEY_CLASSES_ROOT\.mdown" /ve /d "MarkdownReader.Document" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\.mdown" /v "Content Type" /d "text/markdown" /f >nul 2>&1

REM 注册程序信息
reg add "HKEY_CLASSES_ROOT\MarkdownReader.Document" /ve /d "Markdown File" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\MarkdownReader.Document\DefaultIcon" /ve /d "\"%INSTALL_DIR%\MarkdownReader.exe\",0" /f >nul 2>&1
reg add "HKEY_CLASSES_ROOT\MarkdownReader.Document\shell\open\command" /ve /d "\"%INSTALL_DIR%\MarkdownReader.exe\" \"%%1\"" /f >nul 2>&1

echo 文件关联设置完成！
echo.

REM 创建开始菜单快捷方式
echo 创建快捷方式...
set SHORTCUT_DIR=%ProgramData%\Microsoft\Windows\Start Menu\Programs
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT_DIR%\Markdown Reader.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\MarkdownReader.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Markdown Reader'; $Shortcut.Save()"

REM 创建桌面快捷方式
set DESKTOP_DIR=%PUBLIC%\Desktop
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP_DIR%\Markdown Reader.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\MarkdownReader.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Markdown Reader'; $Shortcut.Save()"

echo 快捷方式创建完成！
echo.

REM 添加到卸载程序列表
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "DisplayName" /d "Markdown Reader" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "DisplayVersion" /d "1.0.0" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "Publisher" /d "Your Company" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "InstallLocation" /d "%INSTALL_DIR%" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "UninstallString" /d "\"%INSTALL_DIR%\uninstall.cmd\"" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /v "DisplayIcon" /d "\"%INSTALL_DIR%\MarkdownReader.exe\"" /f >nul 2>&1

REM 创建卸载脚本
echo @echo off > "%INSTALL_DIR%\uninstall.cmd"
echo echo 正在卸载 Markdown Reader... >> "%INSTALL_DIR%\uninstall.cmd"
echo. >> "%INSTALL_DIR%\uninstall.cmd"
echo REM 删除文件关联 >> "%INSTALL_DIR%\uninstall.cmd"
echo reg delete "HKEY_CLASSES_ROOT\.md" /f ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo reg delete "HKEY_CLASSES_ROOT\.markdown" /f ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo reg delete "HKEY_CLASSES_ROOT\.mdown" /f ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo reg delete "HKEY_CLASSES_ROOT\MarkdownReader.Document" /f ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo. >> "%INSTALL_DIR%\uninstall.cmd"
echo REM 删除快捷方式 >> "%INSTALL_DIR%\uninstall.cmd"
echo del "%SHORTCUT_DIR%\Markdown Reader.lnk" ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo del "%DESKTOP_DIR%\Markdown Reader.lnk" ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo. >> "%INSTALL_DIR%\uninstall.cmd"
echo REM 删除注册表项 >> "%INSTALL_DIR%\uninstall.cmd"
echo reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MarkdownReader" /f ^>nul 2^>^&1 >> "%INSTALL_DIR%\uninstall.cmd"
echo. >> "%INSTALL_DIR%\uninstall.cmd"
echo REM 删除程序文件 >> "%INSTALL_DIR%\uninstall.cmd"
echo cd /d "%%TEMP%%" >> "%INSTALL_DIR%\uninstall.cmd"
echo rd /s /q "%INSTALL_DIR%" >> "%INSTALL_DIR%\uninstall.cmd"
echo. >> "%INSTALL_DIR%\uninstall.cmd"
echo echo 卸载完成！ >> "%INSTALL_DIR%\uninstall.cmd"
echo pause >> "%INSTALL_DIR%\uninstall.cmd"

echo.
echo ========================================
echo 安装完成！
echo ========================================
echo.
echo 程序已安装到: %INSTALL_DIR%
echo.
echo 功能:
echo   * 已关联 .md, .markdown, .mdown 文件
echo   * 已创建开始菜单快捷方式
echo   * 已创建桌面快捷方式
echo   * 可通过控制面板卸载
echo.
echo 现在可以双击任何 Markdown 文件打开！
echo.
pause
