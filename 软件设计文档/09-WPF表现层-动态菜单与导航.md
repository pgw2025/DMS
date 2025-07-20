# 09. WPF表现层 - 动态菜单与导航

本文档详细阐述了基于数据库的动态菜单和参数化导航系统的设计方案，旨在与 `iNKORE.UI.WPF.Modern` 等现代化UI框架无缝集成。

## 1. 设计目标

*   **菜单动态化**：应用程序的导航菜单（结构、文本、图标）应由数据库定义，允许在不重新编译程序的情况下进行修改。
*   **视图解耦**：菜单点击（导航发起者）与目标视图（导航接收者）之间不应有直接引用。
*   **参数化导航**：导航时必须能够安全、清晰地将参数（如一个具体的设备ID）传递给目标视图模型。
*   **层级支持**：支持无限层级的父/子菜单结构。

## 2. 数据库设计 (`DbMenu`)

### 2.1. 设计思路与考量

*   **数据驱动**：将菜单的结构、显示文本、图标、目标视图键以及导航参数等信息存储在数据库中。
*   **自引用结构**：通过 `ParentId` 字段实现菜单的层级关系，支持无限层级的子菜单。

### 2.2. 设计优势

*   **高度灵活**：无需修改代码和重新部署应用程序，即可通过修改数据库来调整菜单的显示、顺序、层级和导航目标。
*   **易于管理**：可以通过后台管理界面（如果未来开发）来维护菜单，非开发人员也能操作。
*   **个性化**：理论上可以根据用户权限或配置动态生成不同的菜单。

### 2.3. 设计劣势/权衡

*   **数据库依赖**：菜单的可用性依赖于数据库连接和数据完整性。
*   **性能开销**：每次启动或刷新菜单时，都需要从数据库加载数据并构建菜单树，相比硬编码菜单会有轻微的性能开销。
*   **复杂性增加**：需要额外的数据库表、实体、仓储和构建菜单树的逻辑。

### 2.4. 示例：`DbMenu.cs`

```csharp
// 文件: DMS.Infrastructure/Entities/DbMenu.cs
using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库实体：对应数据库中的 Menus 表，用于存储动态菜单结构。
/// </summary>
[SugarTable("Menus")]
public class DbMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 父菜单的ID。如果为null或0，则为顶级菜单。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? ParentId { get; set; }

    /// <summary>
    /// 显示在UI上的菜单文本。
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    /// 菜单图标。可以使用 Modern UI 框架提供的字形（Glyph）或图像路径。
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 导航目标的唯一键。这是一个字符串，用于在 NavigationService 中映射到具体的ViewModel类型。
    /// 例如："DashboardView", "DeviceListView", "DeviceDetailView"。
    /// </summary>
    public string TargetViewKey { get; set; }

    /// <summary>
    /// (可选) 导航时需要传递的参数。通常以JSON字符串形式存储，由目标ViewModel解析。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NavigationParameter { get; set; }

    /// <summary>
    /// 用于排序，决定同级菜单的显示顺序。
    /// </summary>
    public int DisplayOrder { get; set; }
}
```

## 3. 核心导航契约 (`DMS.WPF`)

### 3.1. `INavigatable` 接口

### 3.1.1. 设计思路与考量

*   **参数化导航**：当导航到某个ViewModel时，可能需要传递特定的数据（如设备ID）。`INavigatable` 接口定义了一个契约，使得任何需要接收导航参数的ViewModel都必须实现 `OnNavigatedToAsync` 方法。
*   **类型安全**：通过 `object parameter` 传递参数，并在 `OnNavigatedToAsync` 内部进行类型检查和转换，确保参数的正确使用。

### 3.1.2. 设计优势

*   **清晰的契约**：明确了ViewModel接收导航参数的方式，提高了代码的可读性和可维护性。
*   **解耦**：导航服务无需知道目标ViewModel的具体实现细节，只需知道它实现了 `INavigatable` 接口。
*   **灵活性**：可以传递任何类型的参数，只要目标ViewModel能够正确解析。

### 3.1.3. 设计劣势/权衡

*   **样板代码**：每个需要接收参数的ViewModel都需要实现 `OnNavigatedToAsync` 方法，并进行参数类型检查。
*   **运行时错误**：如果参数类型不匹配，会在运行时抛出异常，而不是在编译时发现。

### 3.1.4. 示例：`INavigatable.cs`

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

### 3.2. `INavigationService` 接口与实现

### 3.2.1. 设计思路与考量

*   **集中导航逻辑**：将所有导航逻辑封装在一个服务中，而不是分散在各个ViewModel中。
*   **字符串键映射**：使用字符串 `viewKey` 来标识目标ViewModel类型，而不是直接使用 `typeof(ViewModel)`，这使得导航配置可以存储在数据库中。
*   **参数传递**：负责将导航参数从发起者传递给目标ViewModel。

### 3.2.2. 设计优势

*   **解耦**：ViewModel之间不直接进行导航，而是通过 `INavigationService`，降低了耦合度。
*   **可测试性**：可以轻松地Mock `INavigationService`，便于单元测试ViewModel的导航行为。
*   **集中控制**：所有导航规则和逻辑集中管理，便于维护和修改。
*   **支持动态导航**：能够根据数据库配置的 `TargetViewKey` 进行导航。

### 3.2.3. 设计劣势/权衡

*   **抽象开销**：引入了额外的服务层，增加了少量代码量。
*   **映射维护**：`GetViewModelTypeByKey` 方法中的 `switch` 语句需要手动维护 `viewKey` 到 `ViewModel` 类型的映射，当ViewModel数量庞大时，维护成本增加。

### 3.2.4. 示例：`INavigationService.cs`

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

### 3.2.5. 示例：`NavigationService.cs`

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

## 4. 菜单构建与显示

### 4.1. `MenuItemViewModel`

### 4.1.1. 设计思路与考量

*   **UI绑定适配**：`MenuItemViewModel` 是专门为 `iNKORE.UI.WPF.Modern` 的 `NavigationViewItem` 设计的ViewModel。它包含了UI显示所需的属性（如 `Header`, `Icon`）以及导航所需的命令和参数。
*   **命令封装**：每个菜单项都封装了一个 `NavigateCommand`，当点击菜单时，该命令会调用 `INavigationService` 进行导航。

### 4.1.2. 设计优势

*   **MVVM兼容**：完美适配WPF的数据绑定和命令机制。
*   **封装性**：将菜单项的显示逻辑和导航逻辑封装在一起，提高了内聚性。
*   **可重用性**：`MenuItemViewModel` 可以被任何需要显示菜单项的UI组件复用。

### 4.1.3. 设计劣势/权衡

*   **对象开销**：每个菜单项都需要创建一个 `MenuItemViewModel` 实例，对于非常庞大的菜单树，可能会有轻微的内存开销。

### 4.1.4. 示例：`MenuItemViewModel.cs`

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

### 4.2. `IMenuService` (应用层/基础设施层)

### 4.2.1. 设计思路与考量

*   **数据加载**：`IMenuService` 负责从数据库加载 `DbMenu` 记录。
*   **树状构建**：将扁平的 `DbMenu` 列表构建成 `MenuItemViewModel` 的树状结构，以便UI直接绑定。
*   **解耦**：将菜单数据的获取和结构化逻辑与UI层分离。

### 4.2.2. 示例：`IMenuService.cs`

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

### 4.2.3. 示例：`MenuService.cs`

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

## 5. 目标视图模型实现

### 5.1. 设计思路与考量

*   **参数接收**：目标ViewModel通过实现 `INavigatable` 接口来接收导航参数。
*   **数据加载**：在 `OnNavigatedToAsync` 方法中，使用接收到的参数从应用服务加载所需数据。

### 5.2. 示例：`DeviceDetailViewModel.cs`

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

## 6. UI绑定与启动 (`DMS.WPF`)

### 6.1. `MainViewModel`

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

### 6.2. `MainWindow.xaml`

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