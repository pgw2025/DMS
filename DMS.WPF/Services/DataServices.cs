using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 数据服务类，负责从数据库加载和管理各种数据，并提供数据变更通知。
/// 继承自ObservableRecipient，可以接收消息；实现IRecipient<LoadMessage>，处理加载消息。
/// </summary>
public partial class DataServices : ObservableObject
{
    private readonly IMapper _mapper;
    private readonly IDataCenterService _dataCenterService;


    // 设备列表，使用ObservableProperty特性，当值改变时会自动触发属性变更通知。
    [ObservableProperty]
    private ObservableCollection<DeviceItemViewModel> _devices;

    // 变量表列表。
    [ObservableProperty]
    private ObservableCollection<VariableTableItemViewModel> _variableTables;

    // 变量数据列表。
    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _variables;

    // 菜单树列表。
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menus;

    // 菜单树列表。
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menuTrees;

    // MQTT配置列表。
    // [ObservableProperty]
    // private List<Mqtt> _mqtts;



    // 设备列表变更事件，当设备列表数据更新时触发。
    public event Action<List<Device>> OnDeviceListChanged;

    // 菜单树列表变更事件，当菜单树数据更新时触发。
    public event Action<List<MenuBean>> OnMenuTreeListChanged;

    // MQTT列表变更事件，当MQTT配置数据更新时触发。
    // public event Action<List<Mqtt>> OnMqttListChanged;

    // 设备IsActive状态变更事件，当单个设备的IsActive状态改变时触发。
    public event Action<Device, bool> OnDeviceIsActiveChanged;


    /// <summary>
    /// DataServices类的构造函数。
    /// 注入ILogger<DataServices>，并初始化各个数据仓库。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="varDataRepository"></param>
    public DataServices(IMapper mapper, IDataCenterService dataCenterService)
    {
        _mapper = mapper;
        _dataCenterService = dataCenterService;
        Devices = new ObservableCollection<DeviceItemViewModel>();
        VariableTables = new ObservableCollection<VariableTableItemViewModel>();
        Variables = new ObservableCollection<VariableItemViewModel>();
        Menus = new ObservableCollection<MenuItemViewModel>();
        MenuTrees = new ObservableCollection<MenuItemViewModel>();
        // AllVariables = new ConcurrentDictionary<int, Variable>();
    }


    /// <summary>
    /// 异步加载设备数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的设备进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadAllDatas()
    {
        Devices = _mapper.Map<ObservableCollection<DeviceItemViewModel>>(_dataCenterService.Devices.Values);
        VariableTables= _mapper.Map<ObservableCollection<VariableTableItemViewModel>>(_dataCenterService.VariableTables.Values);
        Variables= _mapper.Map<ObservableCollection<VariableItemViewModel>>(_dataCenterService.Variables.Values);
        Menus= _mapper.Map<ObservableCollection<MenuItemViewModel>>(_dataCenterService.Menus.Values);
        MenuTrees= _mapper.Map<ObservableCollection<MenuItemViewModel>>(_dataCenterService.MenuTrees.Values);
        
    }

  
    public void AddMenuItem(MenuItemViewModel menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        var deviceMenu = Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId);
        if (deviceMenu != null)
        {
            deviceMenu.Children.Add(menuItemViewModel);
            Menus.Add(menuItemViewModel);
        }
    }

    public void AddVariableTable(VariableTableItemViewModel variableTableItemViewModel)
    {
        if (variableTableItemViewModel == null)
            return;

        var device = Devices.FirstOrDefault(d => d.Id == variableTableItemViewModel.DeviceId);
        if (device != null)
        {
            device.VariableTables.Add(variableTableItemViewModel);
            VariableTables.Add(variableTableItemViewModel);
        }
    }

    public void AddVariable(VariableItemViewModel variableItem)
    {
        if (variableItem == null)
        {
            return;
        }

        var variableTable = VariableTables.FirstOrDefault(d => d.Id == variableItem.VariableTableId);
        if (variableTable != null)
        {
            variableTable.Variables.Add(variableItem);
            Variables.Add(variableItem);
        }

    }

    public void DeleteMenuItem(MenuItemViewModel menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        // 从扁平菜单列表中移除
        Menus.Remove(menuItemViewModel);

        // 从树形结构中移除
        if (menuItemViewModel.ParentId.HasValue && menuItemViewModel.ParentId.Value != 0)
        {
            // 如果有父菜单，从父菜单的Children中移除
            var parentMenu = Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId.Value);
            parentMenu?.Children.Remove(menuItemViewModel);
        }
        else
        {
            // 如果是根菜单，从MenuTrees中移除
            MenuTrees.Remove(menuItemViewModel);
        }
    }

    public async Task DeleteDeviceById(int selectedDeviceId)
    {
        var device = Devices.FirstOrDefault(d => d.Id == selectedDeviceId);
        if (device != null)
        {
            // 1. 删除与设备关联的所有变量表及其变量
            var variableTablesToDelete = VariableTables.Where(vt => vt.DeviceId == device.Id)
                                                       .ToList();
            foreach (var vt in variableTablesToDelete)
            {
                // 删除与当前变量表关联的所有变量
                var variablesToDelete = Variables.Where(v => v.VariableTableId == vt.Id)
                                                 .ToList();
                foreach (var variable in variablesToDelete)
                {
                    Variables.Remove(variable);
                }

                // 删除变量表
                VariableTables.Remove(vt);

                // 删除与变量表关联的菜单项
                var variableTableMenu
                    = Menus.FirstOrDefault(m => m.TargetViewKey == "VariableTableView" && m.Header == vt.Name);
                if (variableTableMenu != null)
                {
                    DeleteMenuItem(variableTableMenu);
                }
            }

            // 2. 删除设备
            Devices.Remove(device);

            // 3. 删除与设备关联的菜单项
            var deviceMenu = Menus.FirstOrDefault(m => m.TargetViewKey == "DevicesView" && m.Header == device.Name);
            if (deviceMenu != null)
            {
                DeleteMenuItem(deviceMenu);
            }

            // 4. 重新构建菜单树以反映变更
            // BuildMenuTree();
        }
    }

    public void DeleteVariableTableById(int id)
    {
        var variableTable = VariableTables.FirstOrDefault(vt => vt.Id == id);
        if (variableTable != null)
        {
            // 删除与当前变量表关联的所有变量
            var variablesToDelete = Variables.Where(v => v.VariableTableId == variableTable.Id)
                                             .ToList();
            foreach (var variable in variablesToDelete)
            {
                Variables.Remove(variable);
            }


            var device = Devices.FirstOrDefault(d => d.Id == variableTable.DeviceId);
            if (device != null)
                device.VariableTables.Remove(variableTable);
            // 删除变量表
            VariableTables.Remove(variableTable);

            // 删除与变量表关联的菜单项
            var variableTableMenu
                = Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu && m.TargetId == variableTable.Id);
            if (variableTableMenu != null)
            {
                DeleteMenuItem(variableTableMenu);
            }
        }
    }

    public void DeleteVariableById(int id)
    {
        var variableItem = Variables.FirstOrDefault(v => v.Id == id);
        if (variableItem == null)
        {
            return;
        }

        var variableTable = VariableTables.FirstOrDefault(vt => vt.Id == variableItem.VariableTableId);

        variableTable.Variables.Remove(variableItem);

        Variables.Remove(variableItem);


    }
}