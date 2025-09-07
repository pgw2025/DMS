# 项目概述：DMS (Device Management System - 设备管理系统)

此目录包含了设备管理系统 (DMS) 的源代码，这是一个基于 C#/.NET 8 构建的应用程序。该系统主要由一个用于用户界面的 WPF 桌面应用程序组成，后端采用分层架构，包括 Core（核心）、Application（应用）和 Infrastructure（基础设施）项目。此外，还为基础设施层和 WPF 层配备了专门的单元测试项目。

项目自述文件中提到的一个重要特性是 OPC UA 服务实现，表明该系统是为工业设备通信和管理而设计的。

## 项目结构

解决方案 (`DMS.sln`) 由几个关键项目组成：

*   **`DMS.Core`**: 包含基础的、可复用的组件和共享模型。可能包括实用工具、通用数据传输对象 (DTOs) 以及核心业务逻辑抽象。它引用了 `Microsoft.Extensions.Logging.Abstractions`、`Newtonsoft.Json` 和 `NLog`。
*   **`DMS.Application`**: 实现应用特定的业务逻辑。它依赖于 `DMS.Core`，并使用 `AutoMapper`（用于对象映射）和 `Microsoft.Extensions`（用于托管和日志记录抽象）等库。
*   **`DMS.Infrastructure`**: 处理数据访问、外部集成和技术实现。它引用了 `DMS.Core` 和 `DMS.Application`。关键依赖包括：
    *   `SqlSugarCore`：用于数据库交互（MySQL）。
    *   `MQTTnet`：用于 MQTT 协议通信。
    *   `OPCFoundation.NetStandard.Opc.Ua`：用于 OPC UA 通信。
    *   `S7netplus`：用于西门子 S7 PLC 通信。
    *   `NPOI`：用于读写 Excel 文件。
    *   `AutoMapper.Extensions.Microsoft.DependencyInjection`：用于 AutoMapper 的依赖注入设置。
*   **`DMS.WPF`**：主 WPF 桌面应用程序。它以 `net8.0-windows` 为目标，并使用 WPF 特定的库，如 `CommunityToolkit.Mvvm`、`HandyControl`（UI 工具包）、`Hardcodet.NotifyIcon.Wpf`（系统托盘图标）和 `iNKORE.UI.WPF.Modern`（现代 UI 样式）。它依赖于 `DMS.Application` 和 `DMS.Infrastructure`。
*   **`DMS.Infrastructure.UnitTests`**：针对 `DMS.Infrastructure` 项目的单元测试。使用 `xunit`、`Moq`、`Bogus`（用于伪造数据）和 `Microsoft.NET.Test.Sdk`。
*   **`DMS.WPF.UnitTests`**：针对 `DMS.WPF` 项目的单元测试。同样使用 `xunit`、`Moq` 和 `Bogus`。

## 构建与运行

这是一个 .NET 8 解决方案。您需要安装 .NET 8 SDK 以及 Visual Studio 或 JetBrains Rider 等 IDE 来构建和运行它。

**构建：**
在解决方案目录下使用标准 .NET CLI 命令：
`dotnet build`

**运行：**
主可执行文件是 WPF 应用程序 (`DMS.WPF`)。您可以使用以下命令运行它：
`dotnet run --project DMS.WPF`

**测试：**
单元测试包含在 `DMS.Infrastructure.UnitTests` 和 `DMS.WPF.UnitTests` 中。
使用以下命令运行所有测试：
`dotnet test`

## 开发规范

*   **语言与框架：** C# 配合 .NET 8。
*   **架构：** 分层架构（Core, Application, Infrastructure）配合 WPF 表现层。
*   **UI 模式：** MVVM（Model-View-ViewModel），由 `CommunityToolkit.Mvvm` 支持。
*   **依赖注入 (DI)：** 使用 Microsoft.Extensions.DependencyInjection，在 `DMS.WPF` 项目中进行配置。
*   **日志记录：** 使用 NLog 进行日志记录，配置文件位于 `DMS.WPF\Configurations\nlog.config`。
*   **对象映射：** 使用 AutoMapper 进行对象到对象的映射。