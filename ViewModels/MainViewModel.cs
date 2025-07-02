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


    public MainViewModel(NavgatorServices navgatorServices, DataServices dataServices, IDialogService dialogService,
        ILogger<MainViewModel> logger)
    {
        _navgatorServices = navgatorServices;
        _dataServices = dataServices;
        _dialogService = dialogService;
        _logger = logger;


        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };

        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        // 发送消息加载数据
        MessageHelper.SendLoadMessage(LoadTypes.All);
        // 当菜单加载成功后，在前台显示菜单
        dataServices.OnMenuTreeListChanged += (menus) => { Menus = new ObservableCollection<MenuBean>(menus); };
    }

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

                    VariableTableViewModel varTableVM = App.Current.Services.GetRequiredService<VariableTableViewModel>();
                    varTableVM.VariableTable =
                        DataServicesHelper.FindVarTableForDevice(_dataServices.Devices, menu.DataId);
                    menu.ViewModel = varTableVM;
                    menu.Data = varTableVM.VariableTable;
                    
                    break;
                case MenuType.AddVariableTableMenu:
                    await AddVariableTable(menu);
                    break;
            }

            if (menu.Type==MenuType.AddVariableTableMenu)
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
            if (menu.Parent != null && menu.Parent.Data != null)
            {
                Device device = (Device)menu.Parent.Data;
                var varTable = await _dialogService.ShowAddVarTableDialog(device);
                if (varTable != null)
                {
                    // 添加变量表
                    varTable.DeviceId = device.Id;
                    varTable.ProtocolType = device.ProtocolType;
                    var addVarTableId = await new VarTableRepository().Add(varTable);
                    if (addVarTableId > 0)
                    {
                        // 添加变量表菜单
                        MenuBean newMenu = new MenuBean();
                        newMenu.Icon = SegoeFluentIcons.Tablet.Glyph;
                        newMenu.Name = varTable.Name;
                        newMenu.DataId = addVarTableId;
                        newMenu.Type = MenuType.VariableTableMenu;
                        newMenu.ParentId = menu.Parent.Id;
                        var addMenuRes = await new MenuRepository().Add(newMenu);
                        if (addMenuRes > 0)
                        {
                            // 变量表菜单添加成功
                            MessageHelper.SendLoadMessage(LoadTypes.Menu);
                            MessageHelper.SendLoadMessage(LoadTypes.Devices);
                            NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加成功",
                                NotificationType.Success);
                            _logger.LogInformation($"变量表:{varTable.Name},添加成功");
                        }
                        else
                        {
                            // 变量表菜单添加失败
                            NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加菜单失败",
                                NotificationType.Error);
                            _logger.LogError($"变量表:{varTable.Name},添加菜单失败");
                        }
                    }
                    else
                    {
                        // 变量表添加失败
                        NotificationHelper.ShowMessage($"变量表:{varTable.Name},添加失败", NotificationType.Error);
                        _logger.LogError($"变量表:{varTable.Name},添加失败");
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"添加变量表时出现了错误:{e}");
            NotificationHelper.ShowMessage($"添加变量表时出现了错误:{e.Message}", NotificationType.Error);
        }
    }


    public override void OnLoaded()
    {
    }
}