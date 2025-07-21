# 软件开发文档 - 04. DMS.WPF 详细设计 (修订版)

`DMS.WPF` 是系统的用户界面层。本文档详细描述其设计，此修订版特别强调了构建**响应式UI**的核心模式。

## 1. 核心设计模式

### 1.1. MVVM (Model-View-ViewModel)
严格遵循MVVM模式，实现UI与逻辑的彻底分离。

### 1.2. 依赖注入 (Dependency Injection)
使用 `Microsoft.Extensions.DependencyInjection` 统一管理所有服务的生命周期，实现松耦合。

### 1.3. ItemViewModel 模式
**问题**：从应用层获取的DTO（如`DeviceDto`）是简单数据对象，不具备变更通知能力。当后台数据变化时，直接绑定DTO的UI无法自动更新。

**解决方案**：为集合中的每一项数据（如每个设备）创建一个专属的、能发出变更通知的ViewModel。这个`ItemViewModel`会“包裹”原始的DTO，并暴露绑定到UI的属性。当`ItemViewModel`的属性更新时，UI会通过数据绑定自动刷新。

### 1.4. 消息总线 (Messenger)
**问题**：后台服务（如S7通信服务）与UI线程中的ViewModel需要通信（例如，通知设备状态已更新），但它们之间不应有直接引用，否则会造成紧耦合。

**解决方案**：使用消息总线（或事件聚合器）作为解耦的通信中介。`CommunityToolkit.Mvvm` 提供了轻量级的 `IMessenger` 实现。后台服务向总线“广播”消息，所有“订阅”了该消息的ViewModel都会收到通知并做出响应。

## 2. 目录结构 (修订版)

```
DMS.WPF/
├── App.xaml.cs
├── ...
├── Messages/                  <-- (新增) 存放消息总线的消息类
│   └── DeviceStatusChangedMessage.cs
├── Services/
│   ├── ... (导航、对话框服务)
├── ViewModels/
│   ├── Base/                    <-- 存放ViewModel基类和命令
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── Items/                   <-- (新增) 存放所有ItemViewModel
│   │   └── DeviceItemViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── DeviceListViewModel.cs   <-- (已修改)
│   └── MainViewModel.cs
├── Views/
│   └── ...
└── DMS.WPF.csproj
```

## 3. 文件详解

### 3.1. 消息 (`Messages/`)

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

### 3.2. ItemViewModel (`ViewModels/Items/`)

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
    public int Id { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _protocol;

    [ObservableProperty]
    private string _ipAddress;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _status; // 这个属性的改变会立刻反映在UI上

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

### 3.3. 核心视图模型 (`ViewModels/`)

#### `DeviceListViewModel.cs` (修订版)

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

### 3.4. 依赖注入 (`App.xaml.cs`) (修订版)

```csharp
// 文件: DMS.WPF/App.xaml.cs
using CommunityToolkit.Mvvm.Messaging;
// ...其他using

public partial class App : System.Windows.Application
{
    // ...
    private void ConfigureServices(IServiceCollection services)
    {
        // 消息总线 (关键新增)
        // 使用弱引用信使，避免内存泄漏
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // 应用层 & 基础设施层
        // ...

        // WPF UI 服务
        // ...

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DeviceListViewModel>(); // 每次导航都创建新的实例

        // Views
        services.AddSingleton<MainWindow>();
    }
}
```

### 3.5. 视图 (`Views/`)

`DeviceListView.xaml` 的绑定目标现在是 `DeviceItemViewModel` 的属性，无需更改绑定路径，但现在它具备了实时更新的能力。

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