# 实现计划：Markdown 阅读器

## 概述

基于 C#/.NET 8 + WPF + Markdig + WebView2 架构，按增量方式实现 Markdown 阅读器。每个任务在前一个任务基础上构建，确保无孤立代码。

## 任务

- [x] 1. 搭建项目结构和核心框架
  - 创建 WPF 应用项目（.NET 8），命名为 MarkdownReader
  - 创建类库项目 MarkdownReader.Core（存放 Services 和 Models）
  - 创建测试项目 MarkdownReader.Tests（xUnit）
  - 安装 NuGet 包：Markdig、Microsoft.Web.WebView2、CommunityToolkit.Mvvm、FsCheck.Xunit、FluentAssertions
  - 定义所有接口：IMarkdownService、IFileService、ITocService、IThemeService、ISearchService、IExportService、IHtmlTemplateService
  - 定义数据模型：TocItem、SearchResult、ThemeType、AppSettings
  - _需求: 1.1, 1.2_

- [x] 2. 实现 Markdown 解析服务
  - [x] 2.1 实现 MarkdownService
    - 配置 Markdig 管线：UseAdvancedExtensions() + UseMathematics()
    - 实现 ConvertToHtml 方法
    - 实现 ConvertToPlainText 方法
    - _需求: 1.1, 1.2, 1.3, 2.1, 2.2_

  - [x] 2.2 编写属性测试：Markdown 解析往返一致性
    - **Property 1: Markdown 解析往返一致性**
    - **Validates: Requirements 1.1, 1.6**

  - [x] 2.3 编写属性测试：元素到 HTML 标签映射
    - **Property 2: Markdown 元素到 HTML 标签的正确映射**
    - **Validates: Requirements 1.2, 1.3, 1.4**

  - [x] 2.4 编写属性测试：解析器错误容错性
    - **Property 3: 解析器错误容错性**
    - **Validates: Requirements 1.7**

  - [x] 2.5 编写属性测试：数学公式定界符保留
    - **Property 4: 数学公式定界符保留**
    - **Validates: Requirements 2.1, 2.2**

- [x] 3. 实现目录生成服务
  - [x] 3.1 实现 TocService
    - 使用 Markdig AST 遍历提取 HeadingBlock 节点
    - 构建层级 TocItem 树结构
    - 为每个标题生成锚点 ID
    - _需求: 5.1_

  - [x] 3.2 编写属性测试：目录生成正确性
    - **Property 6: 目录生成正确性**
    - **Validates: Requirements 5.1**

- [x] 4. 实现搜索服务
  - [x] 4.1 实现 SearchService
    - 实现大小写不敏感的文本搜索
    - 返回 SearchResult（匹配数量和位置列表）
    - _需求: 6.2, 6.3, 6.5_

  - [x] 4.2 编写属性测试：搜索结果完整性
    - **Property 7: 搜索结果完整性**
    - **Validates: Requirements 6.2, 6.3**

  - [x] 4.3 编写单元测试：搜索边界情况
    - 测试空关键词、空文本、无匹配结果
    - _需求: 6.5_

- [x] 5. 检查点 - 确保所有核心服务测试通过
  - 确保所有测试通过，如有问题请向用户确认。

- [x] 6. 实现主题和文件服务
  - [x] 6.1 实现 ThemeService
    - 实现主题切换逻辑（Light/Dark 切换）
    - 实现 JSON 配置文件读写（AppSettings 序列化/反序列化）
    - 实现 LoadSavedTheme 从配置文件加载
    - 触发 ThemeChanged 事件
    - _需求: 7.1, 7.2, 7.4, 7.5_

  - [x] 6.2 编写属性测试：主题切换对称性
    - **Property 8: 主题切换对称性**
    - **Validates: Requirements 7.2**

  - [x] 6.3 编写属性测试：主题持久化往返一致性
    - **Property 9: 主题持久化往返一致性**
    - **Validates: Requirements 7.4, 7.5**

  - [x] 6.4 实现 FileService
    - 实现 ReadFileAsync 异步文件读取
    - 实现最近文件列表管理（添加、获取、上限 10 条）
    - 实现文件对话框封装（OpenFileDialog、SaveFileDialog）
    - _需求: 4.1, 4.2, 4.4, 4.5, 4.6_

  - [x] 6.5 编写属性测试：最近文件列表不变量
    - **Property 5: 最近文件列表不变量**
    - **Validates: Requirements 4.4**

- [x] 7. 实现 HTML 模板服务
  - [x] 7.1 实现 HtmlTemplateService
    - 构建完整 HTML 页面模板
    - 嵌入 KaTeX CSS/JS 和自动渲染脚本
    - 嵌入 Mermaid.js 初始化脚本
    - 嵌入 highlight.js CSS/JS
    - 根据 ThemeType 切换亮色/暗色样式和 Mermaid 主题
    - _需求: 1.4, 1.5, 2.1, 2.2, 2.3, 3.1, 3.2, 3.3_

  - [x] 7.2 编写单元测试：HTML 模板完整性
    - 验证生成的 HTML 包含 KaTeX、Mermaid、highlight.js 引用
    - 验证主题切换正确应用 CSS 类
    - _需求: 1.4, 2.1, 3.1, 3.3_

- [x] 8. 实现导出服务
  - [x] 8.1 实现 ExportService
    - 实现 ExportToHtmlAsync：生成包含内联样式的独立 HTML 文件
    - 实现 ExportToPdfAsync：调用 WebView2 的 PrintToPdfAsync
    - _需求: 8.1, 8.2, 8.3_

  - [x] 8.2 编写单元测试：HTML 导出
    - 验证导出的 HTML 是完整的独立文件
    - 验证导出错误处理
    - _需求: 8.1, 8.4_

- [x] 9. 检查点 - 确保所有服务层测试通过
  - 确保所有测试通过，如有问题请向用户确认。

- [x] 10. 构建 WPF 主界面
  - [x] 10.1 实现 MainWindow 布局
    - 创建菜单栏（文件、导出、主题）
    - 创建侧边栏（目录 TreeView）
    - 创建主内容区（WebView2 控件）
    - 创建搜索栏（TextBox + 导航按钮）
    - 实现 GridSplitter 可调整侧边栏宽度
    - _需求: 9.1, 9.2, 9.3_

  - [x] 10.2 实现 MainViewModel
    - 绑定文件打开命令（Ctrl+O）
    - 绑定搜索命令（Ctrl+F）
    - 绑定导出命令（Ctrl+E）
    - 绑定主题切换命令
    - 实现文件打开 → 解析 → 渲染完整流程
    - 实现拖放文件支持
    - _需求: 4.1, 4.2, 4.3, 9.4_

  - [x] 10.3 实现 TocViewModel
    - 绑定目录树数据
    - 实现目录项点击 → WebView2 滚动到锚点
    - _需求: 5.2, 5.3_

  - [x] 10.4 实现 SearchViewModel
    - 绑定搜索关键词输入
    - 通过 WebView2 JavaScript 互操作实现搜索高亮
    - 实现上/下导航匹配项
    - _需求: 6.1, 6.2, 6.3, 6.4_

- [x] 11. 集成与完善
  - [x] 11.1 实现最近文件菜单
    - 在文件菜单中显示最近文件列表
    - 点击直接打开对应文件
    - _需求: 4.4, 4.5_

  - [x] 11.2 实现加载指示器
    - 文件加载和解析时显示进度环
    - 完成后自动隐藏
    - _需求: 9.5_

  - [x] 11.3 实现错误处理 UI
    - 文件打开失败显示错误对话框
    - 导出失败显示错误提示
    - _需求: 4.6, 8.4_

- [x] 12. 最终检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请向用户确认。

## 备注

- 标记 `*` 的任务为可选任务，可跳过以加快 MVP 开发
- 每个任务引用了具体的需求编号以确保可追溯性
- 属性测试验证通用正确性属性，单元测试验证具体示例和边界情况
- 检查点确保增量验证
