using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Data;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

/// <summary>
/// 主视图模型，负责应用程序的主导航和数据管理。
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly DataServices _dataServices;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly MenuRepository _menuRepository;
    private readonly NavgatorServices _navgatorServices;
    private readonly VarTableRepository _varTableRepository;

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
    public MainViewModel(NavgatorServices navgatorServices, DataServices dataServices, IDialogService dialogService,
        ILogger<MainViewModel> logger)
    {
        _navgatorServices = navgatorServices;
        _dataServices = dataServices;
        _dialogService = dialogService;
        _logger = logger;
        _varTableRepository = new VarTableRepository();
        _menuRepository = new MenuRepository();

        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };

        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        // 发送消息加载数据
        MessageHelper.SendLoadMessage(LoadTypes.All);
        // 当菜单加载成功后，在前台显示菜单
        dataServices.OnMenuTreeListChanged += (menus) => { Menus = new ObservableCollection<MenuBean>(menus); };
    }

    /// <summary>
    /// 添加变量表。
    /// </summary>
    /// <param name="menu">当前菜单项，用于获取父级设备信息。</param>
    private async Task AddVariableTable(MenuBean menu)
    {
        var db = DbContext.GetInstance();
        try
        {
            // 1. 检查父级设备信息
            if (menu.Parent?.Data is not Device device)
            {
                _logger.LogWarning("尝试添加变量表时，Parent 或 Parent.Data 为空，或 Parent.Data 不是 Device 类型。");
                NotificationHelper.ShowMessage("操作失败：无法获取有效的设备信息。", NotificationType.Error);
                return;
            }


            // 2. 显示添加变量表对话框
            var varTable = await _dialogService.ShowAddVarTableDialog();
            if (varTable == null)
            {
                // 用户取消或未选择
                return;
            }

            // 3. 设置变量表属性
            varTable.IsActive = true;
            varTable.DeviceId = device.Id;
            varTable.ProtocolType = device.ProtocolType;

            // 4. 添加变量表到数据库
            // 假设 _varTableRepository.AddAsync 返回一个布尔值表示成功，或者一个表示 ID 的整数
            // 这里为了演示，我们假设它返回新添加的ID，如果失败则返回0
            await db.BeginTranAsync();
            var addVarTable = await _varTableRepository.Add(varTable,db);

            // 5. 添加变量表菜单
            MenuBean newMenu = new MenuBean
            {
                Icon = SegoeFluentIcons.Tablet.Glyph,
                Name = varTable.Name,
                DataId = addVarTable.Id, // 使用实际添加的ID
                Type = MenuType.VariableTableMenu,
                ParentId = menu.Parent.Id
            };

            var addMenuRes = await _menuRepository.Add(newMenu,db);
            if (addMenuRes > 0)
            {
               await db.CommitTranAsync();
                // 变量表和菜单都添加成功
                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
                NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加成功", NotificationType.Success);
                _logger.LogInformation($"变量表:{varTable.Name},添加成功");
            }
            else
            {
                await db.RollbackTranAsync();
                // 变量表菜单添加失败 (此时变量表可能已添加成功，需要根据业务决定是否回滚)
                NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加菜单失败", NotificationType.Error);
                _logger.LogError($"变量表:{varTable.Name},添加菜单失败");
                // 考虑：如果菜单添加失败，是否需要删除之前添加的变量表？
                // 例如：await _varTableRepository.Delete(addVarTableId);
            }
        }
        catch (Exception e)
        {
            await db.RollbackTranAsync();
            // 捕获并记录所有未预料的异常
            _logger.LogError($"添加变量表时出现了未预期的错误:{e}");
            NotificationHelper.ShowMessage($"添加变量表时出现了错误:{e.Message}", NotificationType.Error);
        }
    }

    /// <summary>
    /// 处理菜单选择变化的逻辑。
    /// </summary>
    /// <param name="menu">被选中的菜单项。</param>
    public async Task MenuSelectionChanged(MenuBean menu)
    {
        try
        {
            switch (menu.Type)
            {
                case MenuType.MainMenu:
                    menu.ViewModel = DataServicesHelper.GetMainViewModel(menu.Name);
                    break;
                case MenuType.DeviceMenu:
                    menu.ViewModel = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
                    menu.Data = _dataServices.Devices.FirstOrDefault(d => d.Id == menu.DataId);
                    break;
                case MenuType.VariableTableMenu:

                    VariableTableViewModel varTableVM =
                        App.Current.Services.GetRequiredService<VariableTableViewModel>();
                    varTableVM.VariableTable =
                        DataServicesHelper.FindVarTableForDevice(_dataServices.Devices, menu.DataId);

                    varTableVM.IsLoadCompletion = false;
                    menu.ViewModel = varTableVM;
                    menu.Data = varTableVM.VariableTable;

                    break;
                case MenuType.AddVariableTableMenu:

                    await AddVariableTable(menu);
                    break;
            }

            if (menu.Type == MenuType.AddVariableTableMenu)
                return;

            if (menu.ViewModel != null)
            {
                MessageHelper.SendNavgatorMessage(menu.ViewModel);
                _logger.LogInformation($"导航到：{menu.Name}");
            }
            else
            {
                NotificationHelper.ShowMessage($"菜单：{menu.Name},没有对应的ViewModel.");
                _logger.LogInformation($"菜单：{menu.Name},没有对应的ViewModel.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"菜单切换是出现了错误:{e}");
            NotificationHelper.ShowMessage($"菜单切换是出现了错误:{e.Message}", NotificationType.Error);
        }
    }
}