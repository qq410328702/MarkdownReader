# 需求文档

## 简介

Markdown 阅读器是一个基于 C# 和 WPF 框架构建的桌面应用程序，使用 Markdig 库解析 Markdown 格式文本并将其渲染为可视化 HTML 内容。用户可以打开本地 Markdown 文件，查看格式化后的内容，并获得良好的阅读体验。应用程序支持 GFM 扩展语法、数学公式、Mermaid 图表、目录导航、主题切换和导出功能。

## 术语表

- **Reader_App**: Markdown 阅读器应用程序的整体系统
- **Markdown_Parser**: 基于 Markdig 库，负责将 Markdown 原始文本解析为 HTML 的组件
- **HTML_Renderer**: 负责在 WebView2 控件中渲染 HTML 内容的组件
- **File_Manager**: 负责文件打开、读取和最近文件管理的组件
- **TOC_Generator**: 负责从 Markdown 标题中提取并生成目录树的组件
- **Theme_Manager**: 负责管理和切换应用程序主题（亮色/暗色）的组件
- **Search_Engine**: 负责在当前文档内容中执行文本搜索的组件
- **Export_Engine**: 负责将 Markdown 内容导出为 HTML 或 PDF 格式的组件
- **Math_Renderer**: 负责将 LaTeX 数学公式渲染为可视化内容的组件（基于 KaTeX 或 MathJax）
- **Mermaid_Renderer**: 负责将 Mermaid 图表定义渲染为 SVG 图形的组件（基于 Mermaid.js）
- **GFM**: GitHub Flavored Markdown，GitHub 扩展的 Markdown 语法规范，包含任务列表、删除线、自动链接、表格等扩展

## 需求

### 需求 1：Markdown 解析与渲染

**用户故事：** 作为用户，我希望系统能够解析标准 Markdown 语法并渲染为格式化的可视内容，以便我能舒适地阅读 Markdown 文档。

#### 验收标准

1. WHEN 用户提供有效的 Markdown 文本, THE Markdown_Parser SHALL 使用 Markdig 库将其解析并转换为 HTML
2. WHEN Markdown 文本包含标准语法元素（标题、段落、粗体、斜体、列表、代码块、链接、图片、引用块、表格、水平线）, THE Markdown_Parser SHALL 正确解析每种元素并生成对应的 HTML 标签
3. WHEN Markdown 文本包含 GFM 扩展语法（任务列表、删除线、自动链接、表格对齐）, THE Markdown_Parser SHALL 启用 Markdig 的 GFM 扩展管线并正确解析这些元素
4. WHEN Markdown 文本包含围栏代码块并指定语言标识, THE HTML_Renderer SHALL 对代码块应用语法高亮样式
5. WHEN HTML 内容准备就绪, THE HTML_Renderer SHALL 在 WebView2 控件中渲染格式化后的内容
6. THE Markdown_Parser SHALL 对有效的 Markdown 文本执行解析后再格式化输出，结果与原始语义等价（往返一致性）
7. IF Markdown 文本包含无法识别的语法, THEN THE Markdown_Parser SHALL 将其作为纯文本处理而不产生错误

### 需求 2：数学公式渲染

**用户故事：** 作为用户，我希望文档中的 LaTeX 数学公式能够被正确渲染，以便我能阅读包含数学内容的技术文档。

#### 验收标准

1. WHEN Markdown 文本包含行内数学公式（单美元符号包裹），THE Math_Renderer SHALL 将其渲染为行内数学符号
2. WHEN Markdown 文本包含块级数学公式（双美元符号包裹），THE Math_Renderer SHALL 将其渲染为居中显示的数学公式块
3. WHEN 数学公式包含常用 LaTeX 语法（分数、上下标、矩阵、求和、积分等），THE Math_Renderer SHALL 正确渲染这些数学符号
4. IF 数学公式包含无效的 LaTeX 语法, THEN THE Math_Renderer SHALL 显示原始公式文本并标记错误提示

### 需求 3：Mermaid 图表渲染

**用户故事：** 作为用户，我希望文档中的 Mermaid 图表定义能够被渲染为可视化图形，以便我能查看流程图、时序图等图表。

#### 验收标准

1. WHEN Markdown 文本包含标记为 mermaid 语言的围栏代码块, THE Mermaid_Renderer SHALL 将其渲染为 SVG 图形
2. WHEN Mermaid 代码块定义了流程图（flowchart）、时序图（sequenceDiagram）、类图（classDiagram）或甘特图（gantt）, THE Mermaid_Renderer SHALL 正确渲染对应类型的图表
3. WHEN 主题切换发生, THE Mermaid_Renderer SHALL 根据当前主题调整图表的配色方案
4. IF Mermaid 代码块包含无效的图表语法, THEN THE Mermaid_Renderer SHALL 显示错误提示信息而非空白区域

### 需求 4：文件管理

**用户故事：** 作为用户，我希望能够方便地打开和管理 Markdown 文件，以便快速访问我的文档。

#### 验收标准

1. WHEN 用户点击"打开文件"按钮或使用快捷键 Ctrl+O, THE File_Manager SHALL 显示文件选择对话框，筛选 .md 和 .markdown 文件
2. WHEN 用户选择一个有效的 Markdown 文件, THE File_Manager SHALL 读取文件内容并传递给 Markdown_Parser 进行解析
3. WHEN 用户通过拖放方式将 Markdown 文件放入应用窗口, THE File_Manager SHALL 读取该文件并触发解析流程
4. WHEN 用户成功打开一个文件, THE File_Manager SHALL 将该文件路径记录到最近文件列表中（最多保留 10 条记录）
5. WHEN 用户访问最近文件列表, THE File_Manager SHALL 显示最近打开的文件路径列表，并允许用户点击直接打开
6. IF 用户尝试打开一个不存在或无法读取的文件, THEN THE File_Manager SHALL 显示明确的错误提示信息

### 需求 5：目录导航

**用户故事：** 作为用户，我希望能够通过目录快速导航到文档的各个章节，以便高效地浏览长文档。

#### 验收标准

1. WHEN 一个 Markdown 文档被成功解析, THE TOC_Generator SHALL 从文档中提取所有标题元素并生成层级目录树
2. WHEN 目录树生成完毕, THE Reader_App SHALL 在侧边栏中显示目录，并按标题层级缩进展示
3. WHEN 用户点击目录中的某个标题项, THE HTML_Renderer SHALL 滚动到文档中对应标题的位置
4. WHEN 文档内容发生变化, THE TOC_Generator SHALL 重新生成目录树以保持同步

### 需求 6：文档内搜索

**用户故事：** 作为用户，我希望能够在当前文档中搜索特定文本，以便快速定位所需内容。

#### 验收标准

1. WHEN 用户按下 Ctrl+F 或点击搜索按钮, THE Reader_App SHALL 显示搜索栏
2. WHEN 用户在搜索栏中输入关键词, THE Search_Engine SHALL 在当前文档内容中查找所有匹配项并高亮显示
3. WHEN 搜索结果存在多个匹配项, THE Search_Engine SHALL 显示匹配数量并支持通过上/下导航按钮在匹配项之间跳转
4. WHEN 用户清空搜索栏或关闭搜索, THE Search_Engine SHALL 移除所有搜索高亮标记
5. IF 搜索关键词在文档中无匹配项, THEN THE Search_Engine SHALL 显示"未找到结果"的提示

### 需求 7：主题切换

**用户故事：** 作为用户，我希望能够在亮色和暗色主题之间切换，以便在不同环境下获得舒适的阅读体验。

#### 验收标准

1. THE Reader_App SHALL 提供亮色和暗色两种主题模式
2. WHEN 用户点击主题切换按钮, THE Theme_Manager SHALL 在亮色和暗色主题之间切换
3. WHEN 主题切换发生, THE Theme_Manager SHALL 同时更新应用程序界面和 HTML 渲染区域的样式
4. WHEN 应用程序启动, THE Theme_Manager SHALL 加载用户上次选择的主题设置
5. WHEN 用户切换主题, THE Theme_Manager SHALL 将当前主题偏好持久化存储到本地配置文件

### 需求 8：导出功能

**用户故事：** 作为用户，我希望能够将 Markdown 文档导出为其他格式，以便分享或打印。

#### 验收标准

1. WHEN 用户选择"导出为 HTML", THE Export_Engine SHALL 生成包含完整样式的独立 HTML 文件
2. WHEN 用户选择"导出为 PDF", THE Export_Engine SHALL 将渲染后的内容转换为 PDF 文件
3. WHEN 导出操作触发, THE Export_Engine SHALL 显示文件保存对话框让用户选择保存位置和文件名
4. IF 导出过程中发生错误, THEN THE Export_Engine SHALL 显示错误信息并保持当前文档状态不变

### 需求 9：用户界面与交互

**用户故事：** 作为用户，我希望应用程序提供直观、响应迅速的界面，以便获得流畅的使用体验。

#### 验收标准

1. THE Reader_App SHALL 使用 WPF 框架构建，提供原生 Windows 桌面应用体验
2. THE Reader_App SHALL 提供包含菜单栏、侧边栏（目录）和主内容区域的布局
3. WHEN 应用窗口大小改变, THE Reader_App SHALL 自适应调整布局，保持内容可读性
4. WHEN 用户使用键盘快捷键（Ctrl+O 打开、Ctrl+F 搜索、Ctrl+E 导出）, THE Reader_App SHALL 执行对应的操作
5. WHEN 文件正在加载或解析中, THE Reader_App SHALL 显示加载指示器以提供反馈
