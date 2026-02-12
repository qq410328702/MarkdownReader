# Markdown Reader 图标生成指南

## 已创建的文件

- **MarkdownReader/app-icon.svg** - SVG 格式的应用图标（矢量图）
- **create-icon.ps1** - 图标转换脚本

## 图标设计说明

SVG 图标包含以下元素：
- 蓝色渐变背景（专业的应用外观）
- 白色 "M" 字母（代表 Markdown）
- 向下箭头（代表文档/阅读）
- 装饰性 Markdown 符号（# 和 **）

## 转换为 ICO 文件

### 方法 1: 使用 ImageMagick（推荐，自动化）

1. 下载并安装 ImageMagick:
   - 访问: https://imagemagick.org/script/download.php#windows
   - 下载 Windows 安装程序
   - 安装时确保勾选 "Install legacy utilities (e.g. convert)"

2. 运行转换脚本:
   ```powershell
   .\create-icon.ps1
   ```

3. 脚本会自动生成包含多种尺寸的 `MarkdownReader/app.ico` 文件

### 方法 2: 在线转换工具（最简单）

1. 打开以下任一网站:
   - https://convertio.co/zh/svg-ico/
   - https://cloudconvert.com/svg-to-ico
   - https://www.aconvert.com/icon/svg-to-ico/
   - https://anyconv.com/svg-to-ico-converter/

2. 上传 `MarkdownReader/app-icon.svg` 文件

3. 选择输出格式为 ICO

4. 下载转换后的文件

5. 将下载的文件重命名为 `app.ico` 并放到 `MarkdownReader` 目录

### 方法 3: 使用 Inkscape（免费开源软件）

1. 下载并安装 Inkscape:
   - 访问: https://inkscape.org/release/
   - 下载并安装 Windows 版本

2. 打开 Inkscape，导入 `app-icon.svg`

3. 导出为 PNG:
   - 文件 → 导出 PNG 图像
   - 设置宽度和高度为 256 像素
   - 导出

4. 使用在线工具将 PNG 转换为 ICO（参考方法 2）

### 方法 4: 使用 GIMP（免费图像编辑器）

1. 下载并安装 GIMP:
   - 访问: https://www.gimp.org/downloads/

2. 打开 GIMP，导入 SVG 文件

3. 调整图像大小为 256x256 像素

4. 导出为 ICO 格式:
   - 文件 → 导出为
   - 选择 .ico 格式
   - 保存

### 方法 5: 使用 Windows Paint 3D

1. 在浏览器中打开 `app-icon.svg`

2. 截图或右键另存为图片

3. 打开 Paint 3D

4. 调整画布大小为 256x256

5. 保存为 PNG

6. 使用在线工具转换为 ICO

## 验证图标

转换完成后，确保：
- 文件名为 `app.ico`
- 文件位置在 `MarkdownReader` 目录
- 文件大小合理（通常 10-100 KB）

可以双击 ICO 文件预览效果。

## 应用图标到项目

图标已经配置在项目中：

1. **应用程序图标** - 在 `MarkdownReader.csproj` 中已配置:
   ```xml
   <ApplicationIcon>app.ico</ApplicationIcon>
   ```

2. **安装包图标** - 在 `Setup/Product.wxs` 中已配置:
   ```xml
   Icon="MarkdownReader.exe"
   ```

3. **文件关联图标** - 安装后，.md 文件会显示应用图标

## 重新构建安装包

创建 ICO 文件后，重新运行构建脚本：

```powershell
.\build-installer.ps1
```

或

```cmd
build-installer.cmd
```

新的安装包将包含你的自定义图标！

## 自定义图标

如果你想修改图标设计：

1. 编辑 `MarkdownReader/app-icon.svg` 文件
2. 可以使用任何文本编辑器或 SVG 编辑器（如 Inkscape）
3. 修改颜色、形状、文字等
4. 重新转换为 ICO 文件

### SVG 颜色参考

当前配置：
- 背景渐变: `#4A90E2` → `#357ABD` (蓝色)
- 前景: `white` (白色)

可以修改为其他颜色方案：
- 绿色: `#4CAF50` → `#388E3C`
- 紫色: `#9C27B0` → `#7B1FA2`
- 橙色: `#FF9800` → `#F57C00`
- 红色: `#F44336` → `#D32F2F`

## 故障排除

### 问题: ICO 文件太大

**解决方案**: 
- 使用在线压缩工具
- 减少 ICO 中包含的尺寸数量
- 只保留 256x256, 48x48, 32x32, 16x16

### 问题: 图标显示模糊

**解决方案**:
- 确保 SVG 转换时使用高分辨率（至少 256x256）
- 使用矢量图形而非位图
- 检查 ICO 文件是否包含多种尺寸

### 问题: 安装后图标不显示

**解决方案**:
- 重启 Windows 资源管理器
- 清除图标缓存
- 重新安装应用程序

## 推荐工具总结

| 工具 | 难度 | 质量 | 推荐度 |
|------|------|------|--------|
| ImageMagick | 简单 | 优秀 | ⭐⭐⭐⭐⭐ |
| 在线转换 | 最简单 | 良好 | ⭐⭐⭐⭐ |
| Inkscape | 中等 | 优秀 | ⭐⭐⭐⭐ |
| GIMP | 中等 | 优秀 | ⭐⭐⭐ |
| Paint 3D | 简单 | 一般 | ⭐⭐ |

## 需要帮助？

如果遇到问题：
1. 运行 `.\create-icon.ps1` 查看详细说明
2. 查看本文档的故障排除部分
3. 使用在线转换工具（最可靠的方法）
