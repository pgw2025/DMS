# 08. WPF表现层 - MVVM与响应式UI

本文档详细设计了 `DMS.WPF` 层的MVVM架构，并阐述了如何通过 `ItemViewModel` 和消息总线构建响应式UI，确保数据变化能实时反映到界面。

## 1. 核心设计模式

### 1.1. MVVM (Model-View-ViewModel)

*   **设计思路**：MVVM 是一种UI架构模式，旨在将UI（View）与业务逻辑和数据（Model）分离。ViewModel 作为View的抽象，负责暴露数据和命令，并处理View的交互逻辑。
*   **优势**：
    *   **职责分离**：View只负责显示，ViewModel负责逻辑，Model负责数据，职责清晰，降低了复杂性。
    *   **可测试性**：ViewModel可以独立于View进行单元测试，无需UI框架的依赖，提高了测试效率和覆盖率。
    *   **可维护性**：UI和逻辑的修改互不影响，降低了维护成本。
    *   **团队协作**：UI设计师和开发者可以并行工作。
*   **劣势/权衡**：
    *   **学习曲线**：对于初学者来说，理解MVVM模式和数据绑定机制需要一定的学习成本。
    *   **样板代码**：需要为每个View创建ViewModel，并实现 `INotifyPropertyChanged` 等接口，增加了少量样板代码（但 `CommunityToolkit.Mvvm` 可以极大简化）。

### 1.2. ItemViewModel 模式

*   **设计思路**：当UI需要显示一个数据集合（如设备列表）时，集合中的每个数据项（如 `DeviceDto`）本身是“哑”的，不具备通知UI更新的能力。`ItemViewModel` 模式为集合中的每个数据项创建一个专属的ViewModel（如 `DeviceItemViewModel`），这个ViewModel“包裹”原始数据，并实现 `INotifyPropertyChanged`。当其内部属性变化时，它会通知UI更新。
*   **优势**：
    *   **响应式UI**：确保数据变化能实时、局部地反映到UI上，提供流畅的用户体验。
    *   **解耦**：将UI更新逻辑封装在 `ItemViewModel` 内部，与原始数据DTO分离。
    *   **可重用性**：`ItemViewModel` 可以在不同的列表或详细视图中复用。
*   **劣势/权衡**：
    *   **对象膨胀**：对于大型集合，每个数据项都创建一个 `ItemViewModel` 实例，可能会增加内存消耗。
    *   **映射开销**：需要将原始数据DTO映射到 `ItemViewModel`，增加了少量代码和运行时开销。

### 1.3. 消息总线 (Messenger)

*   **设计思路**：消息总线（或事件聚合器）是一种发布/订阅模式的实现，用于在应用程序中解耦组件之间的通信。当后台服务（如S7通信服务）检测到设备状态变化时，它不直接调用UI层的ViewModel，而是向消息总线“发布”一条消息。任何“订阅”了该消息的ViewModel都会收到通知并做出响应。
*   **优势**：
    *   **高度解耦**：生产者（消息发布者）和消费者（消息订阅者）之间没有直接引用，降低了模块间的依赖性。
    *   **灵活性**：可以轻松添加新的消息订阅者，而无需修改消息发布者。
    *   **跨层通信**：提供了一种安全、标准的方式进行跨层（如基础设施层到表现层）通信。
*   **劣势/权衡**：
    *   **隐式性**：消息的发布和订阅是隐式的，可能导致代码流向难以追踪，增加了调试难度。
    *   **消息管理**：如果消息类型过多或命名不规范，可能导致混乱。
    *   **内存泄漏风险**：如果订阅者没有正确取消订阅，可能导致内存泄漏（`CommunityToolkit.Mvvm` 的 `WeakReferenceMessenger` 缓解了此问题）。

## 2. 目录结构

```
DMS.WPF/
├── Messages/                  <-- 存放消息类
│   ├── DeviceStatusChangedMessage.cs
│   └── VariableValueUpdatedMessage.cs
├── ViewModels/
│   ├── Base/                    <-- ViewModel基类
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── Items/                   <-- ItemViewModel
│   │   ├── DeviceItemViewModel.cs
│   │   └── VariableItemViewModel.cs
│   ├── DeviceListViewModel.cs
│   └── ...
└── ...
```

## 3. 响应式UI实现流程

### 3.1. 消息定义

```csharp
// 文件: DMS.WPF/Messages/VariableValueUpdatedMessage.cs
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DMS.WPF.Messages;

/// <summary>
/// 当变量值在后台更新时，通过IMessenger广播此消息。
/// </summary>
public class VariableValueUpdatedMessage : ValueChangedMessage<object>
{
    public int VariableId { get; }

    public VariableValueUpdatedMessage(int variableId, object value) : base(value)
    {
        VariableId = variableId;
    }
}
```

### 3.2. ItemViewModel接收消息

`VariableItemViewModel` 实现了 `IRecipient<VariableValueUpdatedMessage>` 接口，当收到匹配的消息时，更新其 `Value` 属性，从而触发UI刷新。

```csharp
// 文件: DMS.WPF/ViewModels/Items/VariableItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.DTOs;
using DMS.WPF.Messages;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表变量列表中的单个变量项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// 同时订阅VariableValueUpdatedMessage以实时更新值。
/// </summary>
public partial class VariableItemViewModel : ObservableObject, IRecipient<VariableValueUpdatedMessage>
{
    public int Id { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private object _value; // 绑定到UI的值，当此属性改变时，UI会自动刷新

    /// <summary>
    /// 构造函数，从DTO创建，并注册消息接收器。
    /// </summary>
    public VariableItemViewModel(VariableDto dto, IMessenger messenger)
    {
        Id = dto.Id;
        _name = dto.Name;
        _value = dto.InitialValue; // 初始值
        messenger.Register<VariableValueUpdatedMessage>(this); // 注册消息接收
    }

    /// <summary>
    /// 实现IRecipient接口，当接收到VariableValueUpdatedMessage消息时此方法被调用。
    /// </summary>
    public void Receive(VariableValueUpdatedMessage message)
    {
        if (message.VariableId == this.Id)
        {
            // 收到匹配的消息，更新值，UI会自动刷新
            Value = message.Value; // ValueChangedMessage 的 Value 属性
        }
    }
}
```

### 3.3. 主ViewModel管理集合

`VariableListViewModel` 持有一个 `ObservableCollection<VariableItemViewModel>`，当从应用层加载数据时，将DTOs转换为 `VariableItemViewModel` 实例并添加到集合中。

```csharp
// 文件: DMS.WPF/ViewModels/VariableListViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DMS.Application.Interfaces;
using DMS.WPF.ViewModels.Items;
using CommunityToolkit.Mvvm.Messaging;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 变量列表视图的ViewModel，管理VariableItemViewModel集合。
/// </summary>
public class VariableListViewModel : BaseViewModel
{
    private readonly IVariableAppService _variableAppService;
    private readonly IMessenger _messenger;

    /// <summary>
    /// 绑定到UI的变量集合。
    /// </summary>
    public ObservableCollection<VariableItemViewModel> Variables { get; } = new();

    /// <summary>
    /// 构造函数。
    /// </summary>
    public VariableListViewModel(IVariableAppService variableAppService, IMessenger messenger)
    {
        _variableAppService = variableAppService;
        _messenger = messenger;
    }

    /// <summary>
    /// 加载变量数据。
    /// </summary>
    public override async Task LoadAsync()
    {
        IsBusy = true;
        Variables.Clear();
        try
        {
            var variableDtos = await _variableAppService.GetAllVariablesAsync(); // 假设有此方法
            foreach (var dto in variableDtos)
            {
                Variables.Add(new VariableItemViewModel(dto, _messenger));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### 3.4. View绑定

XAML中的 `DataGrid` 或 `ItemsControl` 的 `ItemsSource` 绑定到 `ObservableCollection<VariableItemViewModel>`，列表项的属性直接绑定到 `VariableItemViewModel` 的属性。

```xml
<!-- 文件: DMS.WPF/Views/VariableListView.xaml -->
<DataGrid ItemsSource="{Binding Variables}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="名称" Binding="{Binding Name}" />
        <!-- 这个绑定现在可以实时更新了 -->
        <DataGridTextColumn Header="当前值" Binding="{Binding Value}" />
        <!-- ... 其他列 -->
    </DataGrid.Columns>
</DataGrid>
```

## 4. 依赖注入 (`App.xaml.cs`)

### 4.1. 设计思路与考量

*   **标准DI**：使用 `Microsoft.Extensions.DependencyInjection` 作为标准的DI容器。
*   **Messenger注册**：`IMessenger` 必须注册为单例，确保所有组件共享同一个消息总线实例。
*   **ViewModel生命周期**：通常将主窗口的ViewModel注册为单例，而其他子视图的ViewModel注册为 `Transient` 或 `Scoped`，以确保每次导航都获得新的实例。

### 4.2. 示例代码

```csharp
// 文件: DMS.WPF/App.xaml.cs
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace DMS.WPF;

public partial class App : System.Windows.Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // ... NLog初始化

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // 启动后台服务
        _serviceProvider.GetRequiredService<IHostedService>().StartAsync(CancellationToken.None);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 消息总线 (关键新增)
        // 使用弱引用信使，避免内存泄漏
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // 应用层 & 基础设施层服务注册 (示例)
        services.AddTransient<IRepositoryManager, RepositoryManager>();
        services.AddTransient<IDeviceAppService, DeviceAppService>();
        services.AddTransient<IVariableAppService, VariableAppService>();
        services.AddTransient<IMqttPublishService, MqttPublishService>();
        services.AddTransient<IChannelBus, ChannelBusService>();
        services.AddTransient<ILoggerService, NLogService>();

        // 注册后台服务为托管服务
        services.AddHostedService<S7BackgroundService>();

        // WPF UI 服务
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>(); // 假设有此服务

        // ViewModels
        services.AddSingleton<MainViewModel>(); // 主ViewModel通常是单例
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DeviceListViewModel>();
        services.AddTransient<VariableListViewModel>();
        // ... 其他子视图ViewModel注册为Transient

        // Views
        services.AddSingleton<MainWindow>();
    }
}
```