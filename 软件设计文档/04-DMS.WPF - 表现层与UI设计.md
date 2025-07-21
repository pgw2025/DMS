# 软件开发文档 - DMS.WPF - 表现层与UI设计

本文档详细阐述了 `DMS.WPF` 项目的设计，它是系统的用户界面层。本设计严格遵循MVVM (Model-View-ViewModel) 设计模式，并强调了构建响应式UI的核心模式，包括 `ItemViewModel`、消息总线以及数据库驱动的动态菜单和参数化导航系统。

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

### 1.4. 依赖注入 (Dependency Injection)

*   **设计思路**：使用 `Microsoft.Extensions.DependencyInjection` 统一管理所有服务的生命周期，实现松耦合。
*   **优势**：
    *   **松耦合**：组件之间通过接口而非具体实现进行依赖，提高了代码的灵活性和可测试性。
    *   **可维护性**：集中管理对象的创建和生命周期，简化了代码。
*   **劣势/权衡**：
    *   **学习曲线**：对于不熟悉DI的开发者，需要一定的学习成本。
    *   **配置复杂性**：随着项目规模的增大，DI配置可能会变得复杂。

## 2. 目录结构

```
DMS.WPF/
├── App.xaml.cs
├── Assets/
├── Extensions/
├── Helper/
├── Messages/                  <-- 存放消息总线的消息类
│   ├── DeviceStatusChangedMessage.cs
│   ├── VariableValueUpdatedMessage.cs
│   ├── LoadMessage.cs
│   ├── MyMessage.cs
│   ├── NavgatorMessage.cs
│   ├── NotificationMessage.cs
│   ├── ReqMessage.cs
│   └── UpdateMenuMessage.cs
├── Models/
├── Resources/
├── Services/
│   ├── INavigatable.cs
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDialogService.cs
│   ├── DialogService.cs
│   ├── IChannelBus.cs
│   └── ChannelBusService.cs
├── ValueConverts/
├── ViewModels/
│   ├── Base/                    <-- ViewModel基类
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── Items/                   <-- ItemViewModel
│   │   ├── DeviceItemViewModel.cs
│   │   ├── MenuItemViewModel.cs
│   │   └── VariableItemViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── DeviceListViewModel.cs
│   ├── DeviceDetailViewModel.cs
│   ├── VariableListViewModel.cs
│   ├── MqttServerListViewModel.cs
│   ├── MqttServerDetailViewModel.cs
│   └── MainViewModel.cs
├── Views/
│   ├── DashboardView.xaml
│   ├── DeviceListView.xaml
│   ├── DeviceDetailView.xaml
│   ├── VariableListView.xaml
│   ├── MqttServerListView.xaml
│   ├── MqttServerDetailView.xaml
│   └── MainWindow.xaml
└── DMS.WPF.csproj
```

## 3. 总体窗口设计

### 3.1. `SplashWindow.xaml` - 启动加载窗口

*   **View**: 一个简单的窗口，显示Logo和加载状态信息（如“正在连接数据库...”、“正在加载配置...”）。
*   **ViewModel**: `SplashViewModel`
    *   **职责**: 在后台执行初始化任务（如数据库检查、加载配置、连接设备等）。每完成一步，就更新View上显示的状态文本。加载完成后，关闭自身并打开主窗口。

### 3.2. `MainWindow.xaml` - 主窗口

*   **View**: 采用左右布局。
    *   **左侧**: 一个 `ui:NavigationView` 用于显示主菜单。
    *   **右侧**: 一个 `ContentControl`，其 `Content` 属性绑定到当前活动视图模型的View。
*   **ViewModel**: `MainViewModel`
    *   **属性**: 
        *   `ObservableCollection<MenuItemViewModel> MenuItems`: 左侧菜单项集合。
        *   `BaseViewModel CurrentViewModel`: 当前在右侧显示的主视图模型。
    *   **命令**: 
        *   `NavigateCommand`: 当用户点击菜单项时执行，用于切换 `CurrentViewModel`。

## 4. 核心视图与视图模型

### 4.1. 控制台 (Dashboard)

*   **View**: `DashboardView.xaml`
    *   显示多个信息卡片，如“设备总数”、“在线设备”、“离线设备”、“消息日志”等。
*   **ViewModel**: `DashboardViewModel`
    *   **依赖**: `IDeviceAppService`
    *   **属性**: 
        *   `int TotalDeviceCount`
        *   `int OnlineDeviceCount`
        *   `ObservableCollection<string> LogMessages`
    *   **方法**: 
        *   `LoadDataAsync()`: 从应用服务加载统计数据。

### 4.2. 设备管理 (Device Management)

#### 4.2.1. 设备列表视图

*   **View**: `DeviceListView.xaml`
    *   使用 `DataGrid` 显示设备列表。
    *   支持按协议 (`ProtocolType`) 分组。
    *   提供“添加”、“编辑”、“删除”按钮。
    *   双击列表项可导航到设备详情页。
*   **ViewModel**: `DeviceListViewModel`
    *   **依赖**: `IDeviceAppService`, `IDialogService`, `IMessenger`
    *   **属性**: 
        *   `ObservableCollection<DeviceItemViewModel> Devices`
    *   **命令**: 
        *   `AddDeviceCommand`: 打开一个对话框用于添加新设备。
        *   `EditDeviceCommand`: 打开对话框编辑选中设备。
        *   `DeleteDeviceCommand`: 删除选中设备。
        *   `NavigateToDetailCommand`: 导航到变量表视图。

#### 4.2.2. 变量表视图

*   **View**: `VariableTableView.xaml` (假设存在)
    *   显示特定设备下的所有变量表。
    *   显示所选变量表的详细信息。
    *   列表项可点击，导航到变量视图。
*   **ViewModel**: `VariableTableViewModel` (假设存在)
    *   **依赖**: `IVariableTableAppService`
    *   **属性**: 
        *   `DeviceDto CurrentDevice`
        *   `ObservableCollection<VariableTableDto> VariableTables`

### 4.3. MQTT服务器管理

*   **View**: `MqttServerListView.xaml`
    *   `DataGrid` 显示所有已配置的MQTT服务器。
    *   提供增删改功能。
*   **ViewModel**: `MqttServerListViewModel`
    *   **依赖**: `IMqttAppService`
    *   **属性**: 
        *   `ObservableCollection<MqttServerDto> MqttServers`
    *   **命令**: `Add/Edit/Delete` 命令。

#### 4.3.1. MQTT服务器详情页

*   **View**: `MqttServerDetailView.xaml`
    *   显示服务器的连接信息。
    *   显示一个列表，其中包含所有关联到此服务器的变量。
*   **ViewModel**: `MqttServerDetailViewModel`
    *   **依赖**: `IMqttAppService`
    *   **属性**: 
        *   `MqttServerDto CurrentServer`
        *   `ObservableCollection<VariableDto> LinkedVariables`

## 5. 响应式UI实现流程

### 5.1. 消息定义

消息类用于在不同组件之间传递数据，实现解耦通信。

#### `DeviceStatusChangedMessage.cs`

```csharp
// 文件: DMS.WPF/Messages/DeviceStatusChangedMessage.cs
namespace DMS.WPF.Messages;

/// <summary>
/// 当设备状态在后台发生变化时，通过IMessenger广播此消息。
/// </summary>
public class DeviceStatusChangedMessage
{
    /// <summary>
    /// 状态发生变化的设备ID。
    /// </summary>
    public int DeviceId { get; }

    /// <summary>
    /// 设备的新状态文本 (例如: "在线", "离线", "错误")。
    /// </summary>
    public string NewStatus { get; }

    public DeviceStatusChangedMessage(int deviceId, string newStatus)
    {
        DeviceId = deviceId;
        NewStatus = newStatus;
    }
}
```

#### `VariableValueUpdatedMessage.cs`

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

### 5.2. ItemViewModel接收消息

`ItemViewModel` 实现了 `IRecipient<TMessage>` 接口，当收到匹配的消息时，更新其属性，从而触发UI刷新。

#### `DeviceItemViewModel.cs`

```csharp
// 文件: DMS.WPF/ViewModels/Items/DeviceItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表设备列表中的单个设备项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// </summary>
public partial class DeviceItemViewModel : ObservableObject
{
    public int Id { get; } // 设备ID

    [ObservableProperty]
    private string _name; // 设备名称

    [ObservableProperty]
    private string _protocol; // 协议类型

    [ObservableProperty]
    private string _ipAddress; // IP地址

    [ObservableProperty]
    private bool _isActive; // 是否激活

    [ObservableProperty]
    private string _status; // 设备状态，这个属性的改变会立刻反映在UI上

    public DeviceItemViewModel(DeviceDto dto)
    {
        Id = dto.Id;
        _name = dto.Name;
        _protocol = dto.Protocol;
        _ipAddress = dto.IpAddress;
        _isActive = dto.IsActive;
        _status = dto.Status; // 初始状态
    }
}
```

#### `VariableItemViewModel.cs`

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

### 5.3. 主ViewModel管理集合

主ViewModel（如 `DeviceListViewModel` 或 `VariableListViewModel`）持有一个 `ObservableCollection<ItemViewModel>`，当从应用层加载数据时，将DTOs转换为 `ItemViewModel` 实例并添加到集合中。

#### `DeviceListViewModel.cs`

```csharp
// 文件: DMS.WPF/ViewModels/DeviceListViewModel.cs
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.Interfaces;
using DMS.WPF.Messages;
using DMS.WPF.ViewModels.Items;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 设备列表视图的ViewModel，现在接收消息以更新设备状态。
/// </summary>
public class DeviceListViewModel : BaseViewModel, IRecipient<DeviceStatusChangedMessage>
{
    private readonly IDeviceAppService _deviceAppService;

    /// <summary>
    /// 绑定到UI的设备集合，类型已变为DeviceItemViewModel。
    /// </summary>
    public ObservableCollection<DeviceItemViewModel> Devices { get; } = new();

    public DeviceListViewModel(IDeviceAppService deviceAppService, IMessenger messenger)
    {
        _deviceAppService = deviceAppService;
        // 注册消息，以便可以接收到它
        messenger.Register<DeviceStatusChangedMessage>(this);
    }

    public override async Task LoadAsync()
    {
        IsBusy = true;
        Devices.Clear();
        var deviceDtos = await _deviceAppService.GetAllDevicesAsync();
        foreach (var dto in deviceDtos)
        {
            Devices.Add(new DeviceItemViewModel(dto));
        }
        IsBusy = false;
    }

    /// <summary>
    /// 实现IRecipient接口，当接收到DeviceStatusChangedMessage消息时此方法被调用。
    /// </summary>
    public void Receive(DeviceStatusChangedMessage message)
    {
        var deviceToUpdate = Devices.FirstOrDefault(d => d.Id == message.DeviceId);
        if (deviceToUpdate != null)
        {
            // 直接更新ItemViewModel的属性，UI会自动响应！
            deviceToUpdate.Status = message.NewStatus;
        }
    }
}
```

#### `VariableListViewModel.cs`

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

### 5.4. View绑定

XAML中的 `DataGrid` 或 `ItemsControl` 的 `ItemsSource` 绑定到 `ObservableCollection<ItemViewModel>`，列表项的属性直接绑定到 `ItemViewModel` 的属性。

```xml
<!-- 文件: DMS.WPF/Views/DeviceListView.xaml -->
<DataGrid ItemsSource="{Binding Devices}" ...>
    <DataGrid.Columns>
        <DataGridTextColumn Header="名称" Binding="{Binding Name}" />
        <!-- 这个绑定现在是响应式的 -->
        <DataGridTextColumn Header="状态" Binding="{Binding Status}" />
        <!-- ... -->
    </DataGrid.Columns>
</DataGrid>
```

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

## 6. 动态菜单与导航设计

### 6.1. 数据库设计 (`DbMenu`)

将菜单的结构、显示文本、图标、目标视图键以及导航参数等信息存储在数据库中，并通过 `ParentId` 字段实现菜单的层级关系。

#### `DbMenu.cs`

```csharp
// 文件: DMS.Infrastructure/Entities/DbMenu.cs
using SqlSugar;

namespace DMS.Infrastructure.Entities;

[SugarTable("Menus")]
public class DbMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(IsNullable = true)]
    public int? ParentId { get; set; }

    public string Header { get; set; }

    public string Icon { get; set; }

    public string TargetViewKey { get; set; }

    [SugarColumn(IsNullable = true)]
    public string NavigationParameter { get; set; }

    public int DisplayOrder { get; set; }
}
```

### 6.2. 核心导航契约 (`DMS.WPF`)

#### `INavigatable` 接口

任何需要接收导航参数的ViewModel都必须实现此接口。

```csharp
// 文件: DMS.WPF/Services/INavigatable.cs
namespace DMS.WPF.Services;

/// <summary>
/// 定义了一个契约，表示ViewModel可以安全地接收导航传入的参数。
/// </summary>
public interface INavigatable
{
    /// <summary>
    /// 当导航到此ViewModel时，由导航服务调用此方法，以传递参数。
    /// </summary>
    /// <param name="parameter">从导航源传递过来的参数对象。</param>
    Task OnNavigatedToAsync(object parameter);
}
```

#### `INavigationService` 接口与实现

将所有导航逻辑封装在一个服务中，支持基于字符串键和参数的导航。

```csharp
// 文件: DMS.WPF/Services/INavigationService.cs
using System.Threading.Tasks;

namespace DMS.WPF.Services;

/// <summary>
/// 定义了应用程序的导航服务接口。
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 导航到由唯一键标识的视图，并传递一个参数。
    /// </summary>
    /// <param name="viewKey">在DI容器中注册的目标视图的唯一键（通常是ViewModel的名称）。</param>
    /// <param name="parameter">要传递给目标ViewModel的参数。</param>
    Task NavigateToAsync(string viewKey, object parameter = null);
}
```

```csharp
// 文件: DMS.WPF/Services/NavigationService.cs
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.WPF.Services;

/// <summary>
/// INavigationService 的实现，负责解析ViewModel并处理参数传递。
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public NavigationService(IServiceProvider serviceProvider, MainViewModel mainViewModel)
    {
        _serviceProvider = serviceProvider;
        _mainViewModel = mainViewModel;
    }

    /// <summary>
    /// 导航到指定键的视图，并传递参数。
    /// </summary>
    public async Task NavigateToAsync(string viewKey, object parameter = null)
    {
        if (string.IsNullOrEmpty(viewKey))
        {
            // 记录警告或抛出异常
            return;
        }

        // 1. 根据viewKey获取目标ViewModel的Type
        var viewModelType = GetViewModelTypeByKey(viewKey);

        // 2. 从DI容器中解析出ViewModel实例
        // 确保ViewModel被正确注册为Transient或Scoped
        var viewModel = _serviceProvider.GetRequiredService(viewModelType) as BaseViewModel;

        if (viewModel == null)
        {
            // 记录错误：无法解析ViewModel
            throw new InvalidOperationException($"无法解析 ViewModel 类型: {viewModelType.Name}");
        }

        // 3. 如果ViewModel实现了INavigatable接口，则调用其OnNavigatedToAsync方法传递参数
        if (viewModel is INavigatable navigatableViewModel)
        {
            await navigatableViewModel.OnNavigatedToAsync(parameter);
        }

        // 4. 设置为主窗口的当前视图，触发UI更新
        _mainViewModel.CurrentViewModel = viewModel;
    }

    /// <summary>
    /// 将字符串键映射到具体的ViewModel类型。
    /// </summary>
    /// <param name="key">视图键。</param>
    /// <returns>对应的ViewModel类型。</returns>
    /// <exception cref="KeyNotFoundException">如果未找到对应的ViewModel类型。</exception>
    private Type GetViewModelTypeByKey(string key)
    {
        // 这是一个硬编码的映射，可以考虑通过反射或配置进行优化
        return key switch
        {
            "DashboardView" => typeof(DashboardViewModel),
            "DeviceListView" => typeof(DeviceListViewModel),
            "DeviceDetailView" => typeof(DeviceDetailViewModel), // 假设有这个ViewModel
            "VariableListView" => typeof(VariableListViewModel),
            "MqttServerListView" => typeof(MqttServerListViewModel),
            "MqttServerDetailView" => typeof(MqttServerDetailViewModel),
            _ => throw new KeyNotFoundException($"未找到与键 '{key}' 关联的视图模型类型。请检查 NavigationService 的映射配置。"),
        };
    }
}
```

### 6.3. 菜单构建与显示

#### `MenuItemViewModel.cs`

```csharp
// 文件: DMS.WPF/ViewModels/Items/MenuItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表一个可导航的菜单项的ViewModel，用于绑定到UI的NavigationViewItem。
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    private string _icon;

    // 导航目标键和参数，用于传递给 NavigationService
    private readonly string _targetViewKey;
    private readonly object _navigationParameter;

    /// <summary>
    /// 子菜单项集合。
    /// </summary>
    public ObservableCollection<MenuItemViewModel> Children { get; } = new();

    /// <summary>
    /// 菜单项点击时执行的导航命令。
    /// </summary>
    public ICommand NavigateCommand { get; } 

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="header">菜单显示文本。</param>
    /// <param name="icon">菜单图标。</param>
    /// <param name="targetViewKey">导航目标ViewModel的键。</param>
    /// <param name="navigationParameter">导航时传递的参数。</param>
    /// <param name="navigationService">导航服务实例。</param>
    public MenuItemViewModel(string header, string icon, string targetViewKey, object navigationParameter, INavigationService navigationService)
    {
        _header = header;
        _icon = icon;
        _targetViewKey = targetViewKey;
        _navigationParameter = navigationParameter;
        NavigateCommand = new AsyncRelayCommand(async () =>
        {
            await navigationService.NavigateToAsync(_targetViewKey, _navigationParameter);
        });
    }
}
```

#### `IMenuService` (应用层/基础设施层)

此服务负责从数据库加载菜单并构建ViewModel树。

```csharp
// 文件: DMS.Application/Interfaces/IMenuService.cs
using DMS.WPF.ViewModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义了菜单服务接口，用于获取应用程序的导航菜单。
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// 异步获取所有菜单项，并构建成树状结构。
    /// </summary>
    /// <returns>顶级菜单项的列表。</returns>
    Task<List<MenuItemViewModel>> GetMenuItemsAsync();
}
```

```csharp
// 文件: DMS.Infrastructure/Services/MenuService.cs
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Infrastructure.Entities;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services;

/// <summary>
/// IMenuService 的实现，负责从数据库加载菜单并构建 MenuItemViewModel 树。
/// </summary>
public class MenuService : IMenuService
{
    private readonly IRepositoryManager _repoManager;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MenuService(IRepositoryManager repoManager, INavigationService navigationService)
    {
        _repoManager = repoManager;
        _navigationService = navigationService;
    }

    /// <summary>
    /// 异步获取所有菜单项，并构建成树状结构。
    /// </summary>
    public async Task<List<MenuItemViewModel>> GetMenuItemsAsync()
    {
        var allDbMenus = await _repoManager.Menus.GetAllAsync();

        // 将 DbMenu 转换为 MenuItemViewModel，并存储在一个字典中，方便查找
        var menuItemsDict = allDbMenus.ToDictionary(
            m => m.Id,
            m => new MenuItemViewModel(
                m.Header,
                m.Icon,
                m.TargetViewKey,
                // 尝试解析 NavigationParameter 为对象
                string.IsNullOrEmpty(m.NavigationParameter) ? null : JsonSerializer.Deserialize<object>(m.NavigationParameter),
                _navigationService
            )
        );

        var rootMenuItems = new List<MenuItemViewModel>();

        foreach (var dbMenu in allDbMenus)
        {
            if (dbMenu.ParentId.HasValue && menuItemsDict.TryGetValue(dbMenu.ParentId.Value, out var parentMenuItem))
            {
                // 如果有父菜单，则添加到父菜单的Children集合中
                parentMenuItem.Children.Add(menuItemsDict[dbMenu.Id]);
            }
            else
            {
                // 否则，添加到根菜单列表
                rootMenuItems.Add(menuItemsDict[dbMenu.Id]);
            }
        }

        // 根据 DisplayOrder 排序
        return rootMenuItems.OrderBy(m => m.Header).ToList(); // 暂时按Header排序，实际应按DisplayOrder
    }
}
```

### 6.4. 目标视图模型实现

目标ViewModel通过实现 `INavigatable` 接口来接收导航参数，并在 `OnNavigatedToAsync` 方法中加载所需数据。

#### `DeviceDetailViewModel.cs`

```csharp
// 文件: DMS.WPF/ViewModels/DeviceDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 设备详情视图的ViewModel，用于显示单个设备的详细信息。
/// 实现了INavigatable接口以接收导航参数（设备ID）。
/// </summary>
public partial class DeviceDetailViewModel : BaseViewModel, INavigatable
{
    private readonly IDeviceAppService _deviceAppService;

    [ObservableProperty]
    private DeviceDto _device; // 假设有一个DeviceDto用于详情显示

    /// <summary>
    /// 构造函数。
    /// </summary>
    public DeviceDetailViewModel(IDeviceAppService deviceAppService)
    {
        _deviceAppService = deviceAppService;
    }

    /// <summary>
    /// 当导航到此ViewModel时调用，用于加载设备详情。
    /// </summary>
    /// <param name="parameter">导航时传递的设备ID。</param>
    public async Task OnNavigatedToAsync(object parameter)
    {
        // 1. 校验参数类型
        if (parameter is not int deviceId)
        {
            // 如果参数不是期望的int类型，则处理错误（例如，记录日志，导航到错误页面，或显示提示）
            // _logger.Error("导航到DeviceDetailViewModel时参数类型不匹配。");
            return;
        }

        // 2. 使用参数加载数据
        IsBusy = true;
        try
        {
            // 假设IDeviceAppService有GetDeviceDetailAsync方法
            Device = await _deviceAppService.GetDeviceByIdAsync(deviceId); // 使用现有方法
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

## 7. 依赖注入 (`App.xaml.cs`)

使用 `Microsoft.Extensions.DependencyInjection` 作为标准的DI容器。`IMessenger` 必须注册为单例，确保所有组件共享同一个消息总线实例。通常将主窗口的ViewModel注册为单例，而其他子视图的ViewModel注册为 `Transient` 或 `Scoped`。

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
        // _serviceProvider.GetRequiredService<IHostedService>().StartAsync(CancellationToken.None); // 示例，实际可能需要更复杂的启动逻辑
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 消息总线
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // 应用层 & 基础设施层服务注册 (示例)
        // 假设这些服务已在各自的项目中定义并实现了接口
        services.AddTransient<IRepositoryManager, RepositoryManager>();
        services.AddTransient<IDeviceAppService, DeviceAppService>();
        services.AddTransient<IVariableAppService, VariableAppService>();
        services.AddTransient<IMqttPublishService, MqttPublishService>();
        services.AddSingleton<IChannelBus, ChannelBusService>(); // ChannelBus 必须是单例
        services.AddTransient<ILoggerService, NLogService>();
        services.AddTransient<IMenuService, MenuService>();

        // 注册后台服务为托管服务 (如果需要)
        // services.AddHostedService<S7BackgroundService>();

        // WPF UI 服务
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IDialogService, DialogService>(); // 假设有此服务

        // ViewModels
        services.AddSingleton<MainViewModel>(); // 主ViewModel通常是单例
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DeviceListViewModel>();
        services.AddTransient<VariableListViewModel>();
        services.AddTransient<DeviceDetailViewModel>();
        services.AddTransient<MqttServerListViewModel>();
        services.AddTransient<MqttServerDetailViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }
}
```

## 8. UI绑定与启动

### 8.1. `MainViewModel`

`MainViewModel` 负责从 `IMenuService` 加载菜单数据，并将其暴露给 `MainWindow`。

```csharp
// 文件: DMS.WPF/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Items;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 主窗口的ViewModel，管理主界面的导航和菜单。
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IMenuService _menuService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// 绑定到UI的菜单项集合。
    /// </summary>
    public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new();

    [ObservableProperty]
    private BaseViewModel _currentViewModel; // 当前在右侧显示的主视图模型

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MainViewModel(IMenuService menuService, INavigationService navigationService)
    {
        _menuService = menuService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// 加载菜单数据，并在启动时导航到默认视图。
    /// </summary>
    public override async Task LoadAsync()
    {
        IsBusy = true;
        MenuItems.Clear();
        var menus = await _menuService.GetMenuItemsAsync();
        foreach(var menu in menus)
        {
            MenuItems.Add(menu);
        }
        IsBusy = false;

        // 默认导航到控制台视图
        await _navigationService.NavigateToAsync("DashboardView");
    }
}
```

### 8.2. `MainWindow.xaml`

`MainWindow.xaml` 使用 `iNKORE.UI.WPF.Modern` 的 `NavigationView`，并绑定到 `MainViewModel` 的 `MenuItems` 集合。`ContentControl` 绑定到 `CurrentViewModel`，并使用 `DataTemplate` 来根据ViewModel的类型选择对应的View。

```xml
<!-- 文件: DMS.WPF/MainWindow.xaml -->
<Window x:Class="DMS.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:ui="http://schemas.inkore.net/ui/xaml/controls"
        xmlns:vm="clr-namespace:DMS.WPF.ViewModels"
        xmlns:views="clr-namespace:DMS.WPF.Views"
        mc:Ignorable="d"
        Title="设备管理系统"
        Height="768" Width="1024">

    <Window.DataContext>
        <vm:MainViewModel d:IsDataSource="True"/>
    </Window.DataContext>

    <Grid>
        <ui:NavigationView ItemsSource="{Binding MenuItems}"
                           PaneDisplayMode="Left"
                           IsSettingsVisible="False">
            <ui:NavigationView.MenuItemTemplate>
                <DataTemplate>
                    <!-- 使用 HierarchicalDataTemplate 支持子菜单 -->
                    <ui:NavigationViewItem Header="{Binding Header}"
                                           Icon="{Binding Icon}"
                                           Command="{Binding NavigateCommand}"
                                           ItemsSource="{Binding Children}" />
                </DataTemplate>
            </ui:NavigationView.MenuItemTemplate>

            <!-- 右侧内容显示区域 -->
            <ContentControl Content="{Binding CurrentViewModel}">
                <ContentControl.Resources>
                    <!-- DataTemplate 用于将 ViewModel 映射到对应的 View -->
                    <DataTemplate DataType="{x:Type vm:DashboardViewModel}"><views:DashboardView/></DataTemplate>
                    <DataTemplate DataType="{x:Type vm:DeviceListViewModel}"><views:DeviceListView/></DataTemplate>
                    <DataTemplate DataType="{x:Type vm:DeviceDetailViewModel}"><views:DeviceDetailView/></DataTemplate>
                    <DataTemplate DataType="{x:Type vm:VariableListViewModel}"><views:VariableListView/></DataTemplate>
                    <DataTemplate DataType="{x:Type vm:MqttServerListViewModel}"><views:MqttServerListView/></DataTemplate>
                    <DataTemplate DataType="{x:Type vm:MqttServerDetailViewModel}"><views:MqttServerDetailView/></DataTemplate>
                    <!-- ... 其他 ViewModel 到 View 的映射 -->
                </ContentControl.Resources>
            </ContentControl>
        </ui:NavigationView>
    </Grid>
</Window>
```