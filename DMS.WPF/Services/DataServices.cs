using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Helper;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.Helper;
using DMS.Message;
using DMS.WPF.Helper;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace DMS.WPF.Services;

/// <summary>
/// 数据服务类，负责从数据库加载和管理各种数据，并提供数据变更通知。
/// 继承自ObservableRecipient，可以接收消息；实现IRecipient<LoadMessage>，处理加载消息。
/// </summary>
public partial class DataServices : ObservableRecipient, IRecipient<LoadMessage>
{
    private readonly IMapper _mapper;

    private readonly IDeviceAppService _deviceAppService;
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IVariableAppService _variableAppService;

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


    public ConcurrentDictionary<int, Variable> AllVariables;
    private readonly IMenuService _menuService;
    private readonly INavigationService _navigationService;


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
    public DataServices(IMapper mapper, IDeviceAppService deviceAppService,
                        IVariableTableAppService variableTableAppService, IVariableAppService variableAppService,
                        IMenuService menuService, INavigationService navigationService)
    {
        _mapper = mapper;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
        _menuService = menuService;
        _navigationService = navigationService;
        IsActive = true; // 激活消息接收器
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
    public async Task LoadDevices()
    {
        var deviceDtos = await _deviceAppService.GetAllDevicesAsync();
        var deviceDtoIds = new HashSet<int>(deviceDtos.Select(d => d.Id));

        // 1. 更新现有项 & 查找需要删除的项
        var itemsToRemove = new List<DeviceItemViewModel>();
        foreach (var existingItem in Devices)
        {
            if (deviceDtoIds.Contains(existingItem.Id))
            {
                // 设备仍然存在，检查是否有更新
                var dto = deviceDtos.First(d => d.Id == existingItem.Id);

                // 逐一比较属性，只有在发生变化时才更新
                if (existingItem.Name != dto.Name) existingItem.Name = dto.Name;
                if (existingItem.Protocol != dto.Protocol) existingItem.Protocol = dto.Protocol;
                if (existingItem.IpAddress != dto.IpAddress) existingItem.IpAddress = dto.IpAddress;
                if (existingItem.Port != dto.Port) existingItem.Port = dto.Port;
                if (existingItem.Rack != dto.Rack) existingItem.Rack = dto.Rack;
                if (existingItem.Slot != dto.Slot) existingItem.Slot = dto.Slot;
                if (existingItem.OpcUaServerUrl != dto.OpcUaServerUrl) existingItem.OpcUaServerUrl = dto.OpcUaServerUrl;
                if (existingItem.IsActive != dto.IsActive) existingItem.IsActive = dto.IsActive;
                if (existingItem.Status != dto.Status) existingItem.Status = dto.Status;
            }
            else
            {
                // 设备在新列表中不存在，标记为待删除
                itemsToRemove.Add(existingItem);
            }
        }

        // 2. 从UI集合中删除不再存在的项
        foreach (var item in itemsToRemove)
        {
            Devices.Remove(item);
        }

        // 3. 添加新项
        var existingIds = new HashSet<int>(Devices.Select(d => d.Id));
        foreach (var dto in deviceDtos)
        {
            if (!existingIds.Contains(dto.Id))
            {
                // 这是一个新设备，添加到集合中
                var newItem = _mapper.Map<DeviceItemViewModel>(dto);
                Devices.Add(newItem);
            }
        }
    }


    /// <summary>
    /// 异步加载菜单数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的菜单项进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadMenus()
    {
        var newMenus = await _menuService.GetAllMenusAsync(); // 获取最新的菜单树 (MenuItemViewModel 列表)
        var newMenuIds = new HashSet<int>(newMenus.Select(m => m.Id));

        // 1. 更新现有项 & 查找需要删除的项
        var itemsToRemove = new List<MenuItemViewModel>();
        foreach (var existingItem in Menus)
        {
            if (newMenuIds.Contains(existingItem.Id))
            {
                // 菜单项仍然存在，检查是否有更新
                var newDto = newMenus.First(m => m.Id == existingItem.Id);

                // 逐一比较属性，只有在发生变化时才更新
                // 注意：MenuItemViewModel 的属性是 ObservableProperty，直接赋值会触发通知
                if (existingItem.Header != newDto.Header) existingItem.Header = newDto.Header;
                if (existingItem.Icon != newDto.Icon) existingItem.Icon = newDto.Icon;

            }
            else
            {
                // 菜单项在新列表中不存在，标记为待删除
                itemsToRemove.Add(existingItem);
            }
        }

        // 2. 从UI集合中删除不再存在的项
        foreach (var item in itemsToRemove)
        {
            Menus.Remove(item);
        }

        // 3. 添加新项
        var existingIds = new HashSet<int>(Menus.Select(m => m.Id));
        foreach (var newDto in newMenus)
        {
            if (!existingIds.Contains(newDto.Id))
            {
                // 这是一个新菜单项，添加到集合中
                // 注意：这里直接添加 IMenuService 返回的 MenuItemViewModel 实例
                Menus.Add(_mapper.Map<MenuItemViewModel>(newDto));
            }
        }

        BuildMenuTree();
    }

    /// <summary>
    /// 根据扁平菜单列表构建树形结构。
    /// </summary>
    /// <param name="flatMenus">扁平菜单列表。</param>
    /// <returns>树形结构的根菜单列表。</returns>
    private void BuildMenuTree()
    {
        // 1. 创建一个查找表，以便通过ID快速访问菜单项
        var menuLookup = Menus.ToDictionary(m => m.Id);

        // 存储根菜单项的列表
        // var rootMenus = new List<MenuBeanDto>();

        // 2. 遍历所有菜单项，构建树形结构
        foreach (var menu in Menus)
        {
            // 检查是否有父ID，并且父ID不为0（通常0或null表示根节点）
            if (menu.ParentId.HasValue && menu.ParentId.Value != 0)
            {
                // 尝试从查找表中找到父菜单
                if (menuLookup.TryGetValue(menu.ParentId.Value, out var parentMenu))
                {
                    // 将当前菜单添加到父菜单的Children列表中
                    parentMenu.Children.Add(menu);
                }
                // else: 如果找不到父菜单，这可能是一个数据完整性问题，可以根据需要处理
            }
            else
            {
                // 如果没有父ID，则这是一个根菜单
                MenuTrees.Add(menu);
            }
        }
    }


    public void Receive(LoadMessage message)
    {
    }

    /// <summary>
    /// 异步加载变量数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的变量进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadVariables()
    {
        var variableDtos = await _variableAppService.GetAllVariablesAsync(); // 假设有此方法
        var variableDtoIds = new HashSet<int>(variableDtos.Select(v => v.Id));

        // 1. 更新现有项 & 查找需要删除的项
        var itemsToRemove = new List<VariableItemViewModel>();
        foreach (var existingItem in Variables)
        {
            if (variableDtoIds.Contains(existingItem.Id))
            {
                // 变量仍然存在，检查是否有更新
                var dto = variableDtos.First(v => v.Id == existingItem.Id);

                // 逐一比较属性，只有在发生变化时才更新
                if (existingItem.Name != dto.Name) existingItem.Name = dto.Name;
                if (existingItem.S7Address != dto.S7Address) existingItem.S7Address = dto.S7Address;
                if (existingItem.DataValue != dto.DataValue) existingItem.DataValue = dto.DataValue;
                if (existingItem.DisplayValue != dto.DisplayValue) existingItem.DisplayValue = dto.DisplayValue;
                // 注意：VariableTable 和 MqttAliases 是复杂对象，需要更深层次的比较或重新映射
                // 为了简化，这里只比较基本类型属性
                if (existingItem.SignalType != dto.SignalType) existingItem.SignalType = dto.SignalType;
                if (existingItem.PollLevel != dto.PollLevel) existingItem.PollLevel = dto.PollLevel;
                if (existingItem.IsActive != dto.IsActive) existingItem.IsActive = dto.IsActive;
                if (existingItem.VariableTableId != dto.VariableTableId)
                    existingItem.VariableTableId = dto.VariableTableId;
                if (existingItem.OpcUaNodeId != dto.OpcUaNodeId) existingItem.OpcUaNodeId = dto.OpcUaNodeId;
                if (existingItem.IsHistoryEnabled != dto.IsHistoryEnabled)
                    existingItem.IsHistoryEnabled = dto.IsHistoryEnabled;
                if (existingItem.HistoryDeadband != dto.HistoryDeadband)
                    existingItem.HistoryDeadband = dto.HistoryDeadband;
                if (existingItem.IsAlarmEnabled != dto.IsAlarmEnabled) existingItem.IsAlarmEnabled = dto.IsAlarmEnabled;
                if (existingItem.AlarmMinValue != dto.AlarmMinValue) existingItem.AlarmMinValue = dto.AlarmMinValue;
                if (existingItem.AlarmMaxValue != dto.AlarmMaxValue) existingItem.AlarmMaxValue = dto.AlarmMaxValue;
                if (existingItem.AlarmDeadband != dto.AlarmDeadband) existingItem.AlarmDeadband = dto.AlarmDeadband;
                if (existingItem.Protocol != dto.Protocol) existingItem.Protocol = dto.Protocol;
                if (existingItem.CSharpDataType != dto.CSharpDataType) existingItem.CSharpDataType = dto.CSharpDataType;
                if (existingItem.ConversionFormula != dto.ConversionFormula)
                    existingItem.ConversionFormula = dto.ConversionFormula;
                if (existingItem.CreatedAt != dto.CreatedAt) existingItem.CreatedAt = dto.CreatedAt;
                if (existingItem.UpdatedAt != dto.UpdatedAt) existingItem.UpdatedAt = dto.UpdatedAt;
                if (existingItem.UpdatedBy != dto.UpdatedBy) existingItem.UpdatedBy = dto.UpdatedBy;
                if (existingItem.IsModified != dto.IsModified) existingItem.IsModified = dto.IsModified;
                if (existingItem.Description != dto.Description) existingItem.Description = dto.Description;
            }
            else
            {
                // 变量在新列表中不存在，标记为待删除
                itemsToRemove.Add(existingItem);
            }
        }

        // 2. 从UI集合中删除不再存在的项
        foreach (var item in itemsToRemove)
        {
            Variables.Remove(item);
        }

        // 3. 添加新项
        var existingIds = new HashSet<int>(Variables.Select(v => v.Id));
        foreach (var dto in variableDtos)
        {
            if (!existingIds.Contains(dto.Id))
            {
                // 这是一个新变量，添加到集合中
                var newItem = _mapper.Map<VariableItemViewModel>(dto);
                Variables.Add(newItem);
            }
        }
    }

    /// <summary>
    /// 异步加载变量表数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的变量表进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadVariableTables()
    {
        var variableTableDtos = await _variableTableAppService.GetAllVariableTablesAsync(); // 假设有此方法
        var variableTableDtoIds = new HashSet<int>(variableTableDtos.Select(vt => vt.Id));

        // 1. 更新现有项 & 查找需要删除的项
        var itemsToRemove = new List<VariableTableItemViewModel>();
        foreach (var existingItem in VariableTables)
        {
            if (variableTableDtoIds.Contains(existingItem.Id))
            {
                // 变量表仍然存在，检查是否有更新
                var dto = variableTableDtos.First(vt => vt.Id == existingItem.Id);

                // 逐一比较属性，只有在发生变化时才更新
                if (existingItem.Name != dto.Name) existingItem.Name = dto.Name;
                if (existingItem.Description != dto.Description) existingItem.Description = dto.Description;
                if (existingItem.IsActive != dto.IsActive) existingItem.IsActive = dto.IsActive;
                if (existingItem.DeviceId != dto.DeviceId) existingItem.DeviceId = dto.DeviceId;
                if (existingItem.Protocol != dto.Protocol) existingItem.Protocol = dto.Protocol;
            }
            else
            {
                // 变量表在新列表中不存在，标记为待删除
                itemsToRemove.Add(existingItem);
            }
        }

        // 2. 从UI集合中删除不再存在的项
        foreach (var item in itemsToRemove)
        {
            VariableTables.Remove(item);
        }

        // 3. 添加新项
        var existingIds = new HashSet<int>(VariableTables.Select(vt => vt.Id));
        foreach (var dto in variableTableDtos)
        {
            if (!existingIds.Contains(dto.Id))
            {
                // 这是一个新变量表，添加到集合中
                var newItem = _mapper.Map<VariableTableItemViewModel>(dto);
                VariableTables.Add(newItem);
            }
        }
    }

    /// <summary>
    /// 将变量表关联到对应的设备上。
    /// </summary>
    public void AssociateVariableTablesToDevices()
    {
        // 1. 创建一个字典，按 DeviceId 分组所有变量表，以便高效查找
        var variableTablesGroupedByDevice = _variableTables
                                            .GroupBy(vt => vt.DeviceId)
                                            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var device in _devices)
        {
            // 获取当前设备应该关联的所有变量表
            List<VariableTableItemViewModel> associatedVariableTables = new List<VariableTableItemViewModel>();
            if (variableTablesGroupedByDevice.TryGetValue(device.Id, out var foundTables))
            {
                associatedVariableTables = foundTables;
            }

            // 创建一个HashSet，用于快速查找当前设备应有的变量表ID
            var shouldHaveVariableTableIds = new HashSet<int>(associatedVariableTables.Select(vt => vt.Id));

            // 2. 移除不再关联的变量表
            // 从后往前遍历，避免在循环中修改集合导致索引问题
            for (int i = device.VariableTables.Count - 1; i >= 0; i--)
            {
                var existingVariableTable = device.VariableTables[i];
                if (!shouldHaveVariableTableIds.Contains(existingVariableTable.Id))
                {
                    device.VariableTables.RemoveAt(i);
                }
            }

            // 3. 添加新关联的变量表
            var currentlyHasVariableTableIds = new HashSet<int>(device.VariableTables.Select(vt => vt.Id));
            foreach (var newVariableTable in associatedVariableTables)
            {
                if (!currentlyHasVariableTableIds.Contains(newVariableTable.Id))
                {
                    device.VariableTables.Add(newVariableTable);
                }
                // 如果已经存在，则不需要额外操作，因为 LoadVariableTables 已经更新了 _variableTables 中的实例
            }
        }
    }

    /// <summary>
    /// 将变量关联到对应的变量表上。
    /// </summary>
    public void AssociateVariablesToVariableTables()
    {
        // 1. 创建一个字典，按 VariableTableId 分组所有变量，以便高效查找
        var variablesGroupedByVariableTable = _variables
                                              .GroupBy(v => v.VariableTableId)
                                              .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var variableTable in _variableTables)
        {
            // 获取当前变量表应该关联的所有变量
            List<VariableItemViewModel> associatedVariables = new List<VariableItemViewModel>();
            if (variablesGroupedByVariableTable.TryGetValue(variableTable.Id, out var foundVariables))
            {
                associatedVariables = foundVariables;
            }

            // 创建一个HashSet，用于快速查找当前变量表应有的变量ID
            var shouldHaveVariableIds = new HashSet<int>(associatedVariables.Select(v => v.Id));

            // 2. 移除不再关联的变量
            // 从后往前遍历，避免在循环中修改集合导致索引问题
            for (int i = variableTable.Variables.Count - 1; i >= 0; i--)
            {
                var existingVariable = variableTable.Variables[i];
                if (!shouldHaveVariableIds.Contains(existingVariable.Id))
                {
                    variableTable.Variables.RemoveAt(i);
                }
            }

            // 3. 添加新关联的变量
            var currentlyHasVariableIds = new HashSet<int>(variableTable.Variables.Select(v => v.Id));
            foreach (var newVariable in associatedVariables)
            {
                if (!currentlyHasVariableIds.Contains(newVariable.Id))
                {
                    variableTable.Variables.Add(newVariable);
                }
                // 如果已经存在，则不需要额外操作，因为 LoadVariables 已经更新了 _variables 中的实例
            }
        }
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

        VariableTables.Add(variableTableItemViewModel);
        var device = Devices.FirstOrDefault(d => d.Id == variableTableItemViewModel.DeviceId);
        if (device != null)
        {
            device.VariableTables.Add(variableTableItemViewModel);
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
            var variableTablesToDelete = VariableTables.Where(vt => vt.DeviceId == device.Id).ToList();
            foreach (var vt in variableTablesToDelete)
            {
                // 删除与当前变量表关联的所有变量
                var variablesToDelete = Variables.Where(v => v.VariableTableId == vt.Id).ToList();
                foreach (var variable in variablesToDelete)
                {
                    Variables.Remove(variable);
                }

                // 删除变量表
                VariableTables.Remove(vt);

                // 删除与变量表关联的菜单项
                var variableTableMenu = Menus.FirstOrDefault(m => m.TargetViewKey == "VariableTableView" && m.Header == vt.Name);
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
}