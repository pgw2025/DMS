# 软件开发文档 - 05. WPF表现层设计

本文档详细描述 `DMS.WPF` 项目的设计，遵循 MVVM (Model-View-ViewModel) 设计模式。

## 1. 总体窗口设计

### 1.1. `SplashWindow.xaml` - 启动加载窗口

*   **View**: 一个简单的窗口，显示Logo和加载状态信息（如“正在连接数据库...”、“正在加载配置...”）。
*   **ViewModel**: `SplashViewModel`
    *   **职责**: 在后台执行初始化任务（如数据库检查、加载配置、连接设备等）。每完成一步，就更新View上显示的状态文本。加载完成后，关闭自身并打开主窗口。

### 1.2. `MainWindow.xaml` - 主窗口

*   **View**: 采用左右布局。
    *   **左侧**: 一个 `ListBox` 或 `TreeView` 用于显示主菜单。
    *   **右侧**: 一个 `ContentControl`，其 `Content` 属性绑定到当前活动视图模型的View。
*   **ViewModel**: `MainViewModel`
    *   **属性**:
        *   `ObservableCollection<MenuItemViewModel> MenuItems`: 左侧菜单项集合。
        *   `BaseViewModel CurrentViewModel`: 当前在右侧显示的主视图模型。
    *   **命令**:
        *   `NavigateCommand`: 当用户点击菜单项时执行，用于切换 `CurrentViewModel`。

## 2. 核心视图与视图模型

### 2.1. 控制台 (Dashboard)

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

### 2.2. 设备管理 (Device Management)

#### 2.2.1. 设备列表视图

*   **View**: `DeviceListView.xaml`
    *   使用 `GridView` 或 `DataGrid` 显示设备列表。
    *   支持按协议 (`ProtocolType`) 分组。
    *   提供“添加”、“编辑”、“删除”按钮。
    *   双击列表项可导航到设备详情页。
*   **ViewModel**: `DeviceListViewModel`
    *   **依赖**: `IDeviceAppService`, `IDialogService`
    *   **属性**:
        *   `ObservableCollection<DeviceDto> Devices`
    *   **命令**:
        *   `AddDeviceCommand`: 打开一个对话框用于添加新设备。
        *   `EditDeviceCommand`: 打开对话框编辑选中设备。
        *   `DeleteDeviceCommand`: 删除选中设备。
        *   `NavigateToDetailCommand`: 导航到变量表视图。

#### 2.2.2. 变量表视图

*   **View**: `VariableTableView.xaml`
    *   显示特定设备下的所有变量表。
    *   显示所选变量表的详细信息。
    *   列表项可点击，导航到变量视图。
*   **ViewModel**: `VariableTableViewModel`
    *   **依赖**: `IVariableTableAppService`
    *   **属性**:
        *   `DeviceDto CurrentDevice`
        *   `ObservableCollection<VariableTableDto> VariableTables`

### 2.3. MQTT服务器管理

*   **View**: `MqttServerListView.xaml`
    *   `DataGrid` 显示所有已配置的MQTT服务器。
    *   提供增删改功能。
*   **ViewModel**: `MqttServerListViewModel`
    *   **依赖**: `IMqttAppService`
    *   **属性**:
        *   `ObservableCollection<MqttServerDto> MqttServers`
    *   **命令**: `Add/Edit/Delete` 命令。

#### 2.3.1. MQTT服务器详情页

*   **View**: `MqttServerDetailView.xaml`
    *   显示服务器的连接信息。
    *   显示一个列表，其中包含所有关联到此服务器的变量。
*   **ViewModel**: `MqttServerDetailViewModel`
    *   **依赖**: `IMqttAppService`
    *   **属性**:
        *   `MqttServerDto CurrentServer`
        *   `ObservableCollection<VariableDto> LinkedVariables`

## 3. UI服务 (`Services/`)

为了保持ViewModel的整洁，所有与UI直接相关的操作（如弹窗、导航）都应通过服务来完成。

### 3.1. `IDialogService` - 对话框服务

*   **职责**: 负责显示各种对话框（如确认框、信息框、以及用于添加/编辑的自定义对话框）。
*   **方法**:
    *   `Task<bool> ShowConfirmationAsync(string title, string message);`
    *   `Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel);` (用于打开自定义对话框)

### 3.2. `INavigationService` - 导航服务

*   **职责**: 管理主窗口右侧视图的切换。
*   **方法**:
    *   `void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;`

## 4. 依赖注入 (DI)

在 `App.xaml.cs` 的 `OnStartup` 方法中，我们将使用一个DI容器（如 `Microsoft.Extensions.DependencyInjection`）来注册和解析所有服务、视图模型和仓储。

```csharp
// 文件: DMS.WPF/App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    var serviceCollection = new ServiceCollection();
    ConfigureServices(serviceCollection);

    _serviceProvider = serviceCollection.BuildServiceProvider();

    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    mainWindow.Show();
}

private void ConfigureServices(IServiceCollection services)
{
    // 注册应用层服务
    services.AddSingleton<IDeviceAppService, DeviceAppService>();

    // 注册基础设施层服务
    services.AddSingleton<IDeviceRepository, DeviceRepository>();
    services.AddSingleton<IS7CommunicationService, S7CommunicationService>();

    // 注册WPF服务
    services.AddSingleton<IDialogService, DialogService>();

    // 注册ViewModels
    services.AddTransient<MainViewModel>();
    services.AddTransient<DeviceListViewModel>();

    // 注册Views
    services.AddTransient<MainWindow>();
}
```
