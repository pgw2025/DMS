using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavgatorServices _navgatorServices;
    private readonly DataServices _dataServices;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;


    [ObservableProperty] private ViewModelBase currentViewModel;
    [ObservableProperty] private ObservableCollection<MenuBean> _menus;

    private readonly MenuRepository _menuRepository;
    private readonly VarTableRepository _varTableRepository;


    public MainViewModel(NavgatorServices navgatorServices, DataServices dataServices, IDialogService dialogService,
        ILogger<MainViewModel> logger)
    {
        _navgatorServices = navgatorServices;
        _dataServices = dataServices;
        _dialogService = dialogService;
        _logger = logger;
        _varTableRepository = new VarTableRepository();
        _menuRepository= new MenuRepository();

        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };

        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        // 发送消息加载数据
        MessageHelper.SendLoadMessage(LoadTypes.All);
        // 当菜单加载成功后，在前台显示菜单
        dataServices.OnMenuTreeListChanged += (menus) => { Menus = new ObservableCollection<MenuBean>(menus); };
    }

    /// <summary>
    /// 菜单点击切换
    /// </summary>
    /// <param name="menu"></param>
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

    

    private async Task AddVariableTable(MenuBean menu)
    {
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
            var varTable = await _dialogService.ShowAddVarTableDialog(device);
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
            // 假设 _varTableRepository.Add 返回一个布尔值表示成功，或者一个表示 ID 的整数
            // 这里为了演示，我们假设它返回新添加的ID，如果失败则返回0
            var addVarTableId = await _varTableRepository.Add(varTable);
            if (addVarTableId <= 0)
            {
                NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加失败", NotificationType.Error);
                _logger.LogError($"变量表:{varTable.Name},添加失败");
                return; // 添加变量表失败，提前返回
            }

            // 5. 添加变量表菜单
            MenuBean newMenu = new MenuBean
            {
                Icon = SegoeFluentIcons.Tablet.Glyph,
                Name = varTable.Name,
                DataId = addVarTableId, // 使用实际添加的ID
                Type = MenuType.VariableTableMenu,
                ParentId = menu.Parent.Id
            };

            var addMenuRes = await _menuRepository.Add(newMenu);
            if (addMenuRes > 0)
            {
                // 变量表和菜单都添加成功
                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
                NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加成功", NotificationType.Success);
                _logger.LogInformation($"变量表:{varTable.Name},添加成功");
            }
            else
            {
                // 变量表菜单添加失败 (此时变量表可能已添加成功，需要根据业务决定是否回滚)
                NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加菜单失败", NotificationType.Error);
                _logger.LogError($"变量表:{varTable.Name},添加菜单失败");
                // 考虑：如果菜单添加失败，是否需要删除之前添加的变量表？
                // 例如：await _varTableRepository.Delete(addVarTableId);
            }
        }
        catch (Exception e)
        {
            // 捕获并记录所有未预料的异常
            _logger.LogError(e, "添加变量表时出现了未预期的错误。");
            NotificationHelper.ShowMessage($"添加变量表时出现了错误:{e.Message}", NotificationType.Error);
        }
    }
}