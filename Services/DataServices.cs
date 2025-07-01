using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.ViewModels;
using SqlSugar;

namespace PMSWPF.Services;

public partial class DataServices : ObservableRecipient, IRecipient<LoadMessage>
{
    private readonly ILogger<DataServices> _logger;
    [ObservableProperty] private List<Device> _devices;
    [ObservableProperty] private List<VariableTable> _variableTables;
    [ObservableProperty] private List<MenuBean> menuTrees;
    private readonly DeviceRepository _deviceRepository;
    private readonly MenuRepository _menuRepository;

    public event Action<List<Device>> OnDeviceListChanged;
    public event Action<List<MenuBean>> OnMenuTreeListChanged;


    partial void OnDevicesChanged(List<Device> devices)
    {
        OnDeviceListChanged?.Invoke(devices);
        if (menuTrees != null && Devices != null)
        {
            FillMenuData(MenuTrees, Devices);
        }
    }

    partial void OnMenuTreesChanged(List<MenuBean> MenuTrees)
    {
        OnMenuTreeListChanged?.Invoke(MenuTrees);
        if (MenuTrees != null && Devices != null)
        {
            FillMenuData(MenuTrees, Devices);
        }
    }


    public DataServices(ILogger<DataServices> logger)
    {
        _logger = logger;
        IsActive = true;
        _deviceRepository = new DeviceRepository();
        _menuRepository = new MenuRepository();
    }


    /// <summary>
    /// 给Menu菜单的Data填充数据
    /// </summary>
    /// <param name="MenuTrees"></param>
    private void FillMenuData(List<MenuBean> MenuTrees, List<Device> devices)
    {
        if (MenuTrees == null || MenuTrees.Count == 0)
            return;

        foreach (MenuBean menuBean in MenuTrees)
        {
            switch (menuBean.Type)
            {
                case MenuType.MainMenu:
                    menuBean.ViewModel =DataServicesHelper.GetMainViewModel(menuBean.Name);
                    break;
                case MenuType.DeviceMenu:
                    menuBean.ViewModel = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
                    menuBean.Data = devices.FirstOrDefault(d => d.Id == menuBean.DataId);
                    break;
                case MenuType.VariableTableMenu:
                    var varTableVM = App.Current.Services.GetRequiredService<VariableTableViewModel>();
                    varTableVM.VariableTable = FindVarTableForDevice(menuBean.DataId);
                    menuBean.ViewModel = varTableVM;
                    menuBean.Data = varTableVM.VariableTable;
                    break;
                case MenuType.AddVariableTableMenu:
                    break;
            }

            if (menuBean.Items != null && menuBean.Items.Count > 0)
            {
                FillMenuData(menuBean.Items, devices);
            }
        }
    }

    

    /// <summary>
    /// 查找设备所对应的菜单对象
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public async Task<int> UpdateMenuForDevice(Device device)
    {
       var menu= DataServicesHelper.FindMenusForDevice(device, MenuTrees);
       if (menu != null)
          return await _menuRepository.Edit(menu);

        return 0;
    }

    /// <summary>
    /// 从设备列表中找到变量表VarTable对象
    /// </summary>
    /// <param name="vtableId">VarTable的ID</param>
    /// <returns>如果找到择返回对象，否则返回null</returns>
    private VariableTable FindVarTableForDevice(int vtableId)
    {
        VariableTable varTable = null;
        foreach (var device in _devices)
        {
            varTable = device.VariableTables.FirstOrDefault(v => v.Id == vtableId);
            if (varTable != null)
                return varTable;
        }

        return varTable;
    }

    /// <summary>
    /// 接受加载消息，收到消息后从数据库加载对应的数据
    /// </summary>
    /// <param name="message">消息的类型，如加载菜单LoadMessage.Menu</param>
    /// <exception cref="ArgumentException"></exception>
    public async void Receive(LoadMessage message)
    {
        if (!(message.Value is LoadTypes))
            throw new ArgumentException($"接受到的加载类型错误：{message.Value}");
        try
        {
            switch ((LoadTypes)message.Value)
            {
                case LoadTypes.All:
                    Devices = await _deviceRepository.GetAll();
                    await LoadMenus();
                    break;
                case LoadTypes.Devices:
                    Devices = await _deviceRepository.GetAll();
                    break;
                case LoadTypes.Menu:
                    await LoadMenus();
                    break;
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"加载数据出现了错误：{e.Message}");
            _logger.LogError($"加载数据出现了错误：{e}");
        }
    }

    private async Task LoadMenus()
    {
        MenuTrees = await _menuRepository.GetMenuTrees();
        foreach (MenuBean menu in MenuTrees)
        {
            MenuHelper.MenuAddParent(menu);
        }
    }

    public async Task<int> DeleteMenuForDevice(Device device)
    {
        var menu= DataServicesHelper.FindMenusForDevice(device, MenuTrees);
        if (menu != null)
        {
           return await _menuRepository.DeleteMenu(menu);
        }

        return 0;

    }
}