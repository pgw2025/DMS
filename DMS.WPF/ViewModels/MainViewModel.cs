using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.WPF.Services;
using DMS.WPF.Views;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Helper;
using DMS.Services;
using DMS.ViewModels;
using DMS.WPF.Helper;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// AddAsync this using directive
// AddAsync this using directive

namespace DMS.WPF.ViewModels;

/// <summary>
/// 主视图模型，负责应用程序的主导航和数据管理。
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    public  DataServices DataServices { get; }
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;

    /// <summary>
    /// 当前显示的视图模型。
    /// </summary>
    [ObservableProperty]
    private ViewModelBase currentViewModel;

    /// <summary>
    /// 应用程序的菜单列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MenuBean> _menus;

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="navgatorServices">导航服务。</param>
    /// <param name="dataServices">数据服务。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <param name="logger">日志记录器。</param>
    public MainViewModel(DataServices dataServices,
                         ILogger<MainViewModel> logger)
    {
        DataServices = dataServices;
        _logger = logger;

        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        // 发送消息加载数据
        MessageHelper.SendLoadMessage(LoadTypes.All);
        // 当菜单加载成功后，在前台显示菜单
        // dataServices.OnMenuTreeListChanged += (menus) => { Menus = new ObservableCollection<MenuBean>(menus); };
    }

    /// <summary>
    /// 显示主窗口的命令。
    /// </summary>
    [RelayCommand]
    private void ShowWindow()
    {
        // if (Application.Current.MainWindow is MainView mainWindow)
        // {
        //     mainWindow.ShowApplication();
        // }
    }

    /// <summary>
    /// 退出应用程序的命令。
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        // Application.Current.Shutdown();
    }

    /// <summary>
    /// 添加变量表。
    /// </summary>
    /// <param name="menu">当前菜单项，用于获取父级设备信息。</param>
    private async Task AddVariableTable(MenuBean menu)
    {
        // var db = DbContext.GetInstance();
        // try
        // {
        //     // 1. 检查父级设备信息
        //     if (menu.Parent?.Data is not Device device)
        //     {
        //         _logger.LogWarning("尝试添加变量表时，Parent 或 Parent.Data 为空，或 Parent.Data 不是 Device 类型。");
        //         NotificationHelper.ShowError("操作失败：无法获取有效的设备信息。");
        //         return;
        //     }
        //
        //
        //     // 2. 显示添加变量表对话框
        //     var varTable = await _dialogService.ShowAddVarTableDialog();
        //     if (varTable == null)
        //     {
        //         // 用户取消或未选择
        //         return;
        //     }
        //
        //     // 3. 设置变量表属性
        //     varTable.IsActive = true;
        //     varTable.DeviceId = device.Id;
        //     varTable.ProtocolType = device.ProtocolType;
        //
        //     // 4. 添加变量表到数据库
        //     // 假设 _varTableRepository.AddAsync 返回一个布尔值表示成功，或者一个表示 ID 的整数
        //     // 这里为了演示，我们假设它返回新添加的ID，如果失败则返回0
        //     await db.BeginTranAsync();
        //     var addVarTable = await _varTableRepository.AddAsync(varTable, db);
        //
        //     // 5. 添加变量表菜单
        //     MenuBean newMenu = new MenuBean
        //                        {
        //                            Icon = SegoeFluentIcons.Tablet.Glyph,
        //                            Name = varTable.Name,
        //                            DataId = addVarTable.Id, // 使用实际添加的ID
        //                            Type = MenuType.VariableTableMenu,
        //                            ParentId = menu.Parent.Id
        //                        };
        //
        //     var addMenuRes = await _menuRepository.AddAsync(newMenu, db);
        //     if (addMenuRes > 0)
        //     {
        //         await db.CommitTranAsync();
        //         // 变量表和菜单都添加成功
        //         MessageHelper.SendLoadMessage(LoadTypes.Menu);
        //         MessageHelper.SendLoadMessage(LoadTypes.Devices);
        //         NotificationHelper.ShowSuccess($"变量表:{varTable.Name},添加成功");
        //         _logger.LogInformation($"变量表:{varTable.Name},添加成功");
        //     }
        //     else
        //     {
        //         await db.RollbackTranAsync();
        //         // 变量表菜单添加失败 (此时变量表可能已添加成功，需要根据业务决定是否回滚)
        //         NotificationHelper.ShowError($"变量表:{varTable.Name},添加菜单失败");
        //         _logger.LogError($"变量表:{varTable.Name},添加菜单失败");
        //         // 考虑：如果菜单添加失败，是否需要删除之前添加的变量表？
        //         // 例如：await _varTableRepository.DeleteAsync(addVarTableId);
        //     }
        // }
        // catch (Exception e)
        // {
        //     await db.RollbackTranAsync();
        //     NotificationHelper.ShowError($"添加变量表时出现了错误:{e.Message}", e);
        // }
    }

    /// <summary>
    /// 处理菜单选择变化的逻辑。
    /// </summary>
    /// <param name="menu">被选中的菜单项。</param>
    public async Task MenuSelectionChanged(MenuBean menu)
    {
        // try
        // {
        //     switch (menu.Type)
        //     {
        //         // 导航到一级菜单
        //         case MenuType.MainMenu:
        //             menu.ViewModel = DataServicesHelper.GetMainViewModel(menu.Name);
        //             break;
        //         // 导航到设备下面的菜单
        //         case MenuType.DeviceMenu:
        //             var deviceDetailVm = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
        //             var currentDevice = _dataServices.Devices.FirstOrDefault(d => d.Id == menu.DataId);
        //             deviceDetailVm.CurrentDevice = currentDevice;
        //             menu.ViewModel = deviceDetailVm;
        //             menu.Data = currentDevice;
        //             break;
        //         // 导航到变量表菜单
        //         case MenuType.VariableTableMenu:
        //             VariableTableViewModel varTableVM =
        //                 App.Current.Services.GetRequiredService<VariableTableViewModel>();
        //             varTableVM.VariableTable =
        //                 DataServicesHelper.FindVarTableForDevice(_dataServices.Devices, menu.DataId);
        //
        //             varTableVM.IsLoadCompletion = false;
        //             menu.ViewModel = varTableVM;
        //             menu.Data = varTableVM.VariableTable;
        //             break;
        //         // 导航到添加变量表的菜单
        //         case MenuType.AddVariableTableMenu:
        //             await AddVariableTable(menu);
        //             return;
        //             break;
        //         // 导航到Mqtt服务器
        //         case MenuType.MqttMenu:
        //             var mqttVM = App.Current.Services.GetRequiredService<MqttServerDetailViewModel>();
        //             mqttVM.CurrentMqtt = _dataServices.Mqtts.FirstOrDefault(d => d.Id == menu.DataId);
        //             menu.ViewModel = mqttVM;
        //             break;
        //     }
        //
        //
        //     if (menu.ViewModel != null)
        //     {
        //         MessageHelper.SendNavgatorMessage(menu.ViewModel);
        //         _logger.LogInformation($"导航到：{menu.Name}");
        //     }
        //     else
        //     {
        //         NotificationHelper.ShowInfo($"菜单：{menu.Name},没有对应的ViewModel.");
        //         _logger.LogInformation($"菜单：{menu.Name},没有对应的ViewModel.");
        //     }
        // }
        // catch (Exception e)
        // {
        //     NotificationHelper.ShowError($"菜单切换时出现了错误:{e.Message}", e);
        // }
    }
}