# Markdown Reader 安装包构建指南

## 前置要求

1. **.NET 8.0 SDK** - 已安装
2. **WiX Toolset v3.11+** - 需要安装

### 安装 WiX Toolset

下载并安装 WiX Toolset:
- 官方网站: https://wixtoolset.org/releases/
- 推荐版本: WiX Toolset v3.11 或更高版本
- 下载 `.exe` 安装程序并运行

## 构建安装包

### 方法 1: 使用 PowerShell 脚本（推荐）

```powershell
.\build-installer.ps1
```

### 方法 2: 使用 CMD 脚本

```cmd
build-installer.cmd
```

### 方法 3: 手动构建

```powershell
# 1. 发布应用程序
dotnet publish .\MarkdownReader\MarkdownReader.csproj -c Release -r win-x64 --self-contained true

# 2. 编译 WiX 文件
candle.exe -dPublishDir=".\MarkdownReader\bin\Release\net8.0-windows\win-x64\publish" -out ".\Setup\obj\\" ".\Setup\Product.wxs" ".\Setup\Files.wxs"

# 3. 生成 MSI 安装包
light.exe -out ".\Setup\bin\MarkdownReaderSetup.msi" -ext WixUIExtension ".\Setup\obj\Product.wixobj" ".\Setup\obj\Files.wixobj"
```

## 安装包功能

构建完成后，会在 `Setup\bin\` 目录下生成 `MarkdownReaderSetup.msi` 安装包，包含以下功能：

✅ **文件关联**
- 自动关联 `.md` 文件
- 自动关联 `.markdown` 文件
- 自动关联 `.mdown` 文件
- 双击 Markdown 文件即可用 Markdown Reader 打开

✅ **快捷方式**
- 在开始菜单创建快捷方式
- 在桌面创建快捷方式

✅ **完整卸载支持**
- 通过控制面板完全卸载
- 自动清理文件关联和快捷方式

## 安装和使用

1. 双击 `MarkdownReaderSetup.msi`
2. 按照安装向导完成安装
3. 安装完成后，双击任何 `.md` 文件即可用 Markdown Reader 打开

## 卸载

通过 Windows 设置 → 应用 → 已安装的应用 → Markdown Reader → 卸载

## 自定义配置

### 修改版本号

编辑 `MarkdownReader\MarkdownReader.csproj`:
```xml
<Version>1.0.0</Version>
```

编辑 `Setup\Product.wxs`:
```xml
<Product Version="1.0.0.0" ... >
```

### 修改公司信息

编辑 `MarkdownReader\MarkdownReader.csproj`:
```xml
<Authors>Your Name</Authors>
<Company>Your Company</Company>
```

编辑 `Setup\Product.wxs`:
```xml
<Product Manufacturer="Your Company" ... >
```

### 添加应用图标

1. 将图标文件命名为 `app.ico` 放在 `MarkdownReader` 目录
2. 图标会自动应用到程序和文件关联

## 故障排除

### 问题: 找不到 WiX Toolset

**解决方案**: 
- 确保已安装 WiX Toolset
- 检查环境变量 `%WIX%` 是否正确设置
- 重启命令行窗口

### 问题: 发布失败

**解决方案**:
- 确保已安装 .NET 8.0 SDK
- 运行 `dotnet --version` 检查版本
- 清理项目: `dotnet clean`

### 问题: 文件关联不生效

**解决方案**:
- 以管理员身份运行安装程序
- 安装后重启 Windows 资源管理器
- 检查默认程序设置

## 高级选项

### 创建单文件发布

如果想要单个 .exe 文件，修改发布命令:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

注意: 单文件发布需要相应修改 `Setup\Files.wxs`

### 添加更多文件扩展名

编辑 `Setup\Product.wxs`，添加新的 `<ProgId>` 和 `<Extension>` 节点。

## 技术支持

如有问题，请查看:
- WiX 文档: https://wixtoolset.org/documentation/
- .NET 发布文档: https://learn.microsoft.com/dotnet/core/deploying/
