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
    private ObservableCollection<MenuBeanItemViewModel> _menus;

    // MQTT配置列表。
    // [ObservableProperty]
    // private List<Mqtt> _mqtts;


    public ConcurrentDictionary<int, Variable> AllVariables;
    private readonly IMenuService _menuService;


    // 设备列表变更事件，当设备列表数据更新时触发。
    public event Action<List<Device>> OnDeviceListChanged;

    // 菜单树列表变更事件，当菜单树数据更新时触发。
    public event Action<List<MenuBean>> OnMenuTreeListChanged;

    // MQTT列表变更事件，当MQTT配置数据更新时触发。
    // public event Action<List<Mqtt>> OnMqttListChanged;

    // 设备IsActive状态变更事件，当单个设备的IsActive状态改变时触发。
    public event Action<Device, bool> OnDeviceIsActiveChanged;


    // /// <summary>
    // /// 当_mqtts属性值改变时触发的局部方法，用于调用OnMqttListChanged事件。
    // /// </summary>
    // /// <param name="mqtts">新的MQTT配置列表。</param>
    // partial void OnMqttsChanged(List<Mqtt> mqtts)
    // {
    //     OnMqttListChanged?.Invoke(mqtts);
    // }

    // 注释掉的代码块，可能用于变量数据变更事件的触发，但目前未启用。
    // {
    //     OnVariableDataChanged?.Invoke(this, value);
    // }

    /// <summary>
    /// DataServices类的构造函数。
    /// 注入ILogger<DataServices>，并初始化各个数据仓库。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="varDataRepository"></param>
    public DataServices(IMapper mapper, IDeviceAppService deviceAppService,
                        IVariableTableAppService variableTableAppService, IVariableAppService variableAppService,IMenuService menuService)
    {
        _mapper = mapper;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
        _menuService = menuService;
        IsActive = true; // 激活消息接收器
        Devices = new ObservableCollection<DeviceItemViewModel>();
        VariableTables = new ObservableCollection<VariableTableItemViewModel>();
        Variables = new ObservableCollection<VariableItemViewModel>();
        Menus = new ObservableCollection<MenuBeanItemViewModel>();
        // AllVariables = new ConcurrentDictionary<int, Variable>();
    }

    // /// <summary>
    // /// 接收加载消息，根据消息类型从数据库加载对应的数据。
    // /// </summary>
    // /// <param name="message">加载消息，包含要加载的数据类型。</param>
    // /// <exception cref="ArgumentException">如果加载类型未知，可能会抛出此异常（尽管当前实现中未显式抛出）。</exception>
    // public async void Receive(LoadMessage message)
    // {
    //     try
    //     {
    //         switch (message.LoadType)
    //         {
    //             case LoadTypes.All: // 加载所有数据
    //                 await LoadDevices();
    //                 await LoadMenus();
    //                 await LoadMqtts();
    //                 break;
    //             case LoadTypes.Devices: // 仅加载设备数据
    //                 await LoadDevices();
    //                 break;
    //             case LoadTypes.Menu: // 仅加载菜单数据
    //                 await LoadMenus();
    //                 break;
    //             case LoadTypes.Mqtts: // 仅加载MQTT配置数据
    //                 await LoadMqtts();
    //                 break;
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         // 捕获加载数据时发生的异常，并通过通知和日志记录错误信息。
    //         NotificationHelper.ShowError($"加载数据出现了错误：{e.Message}", e);
    //     }
    // }

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

    // private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    // {
    //     if (e.PropertyName == nameof(Device.IsActive))
    //     {
    //         if (sender is Device device)
    //         {
    //             NlogHelper.Info($"设备 {device.Name} 的IsActive状态改变为 {device.IsActive}，触发设备IsActive状态变更事件。");
    //             OnDeviceIsActiveChanged?.Invoke(device, device.IsActive);
    //         }
    //     }
    // }

    /// <summary>
    /// 异步加载菜单数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的菜单项进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadMenus()
    {
        var newMenus = await _menuService.GetAllMenusAsync(); // 获取最新的菜单树 (MenuItemViewModel 列表)
        var newMenuIds = new HashSet<int>(newMenus.Select(m => m.Id));

        // 1. 更新现有项 & 查找需要删除的项
        var itemsToRemove = new List<MenuBeanItemViewModel>();
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
                // 对于 TargetViewKey 和 NavigationParameter，它们在 MenuItemViewModel 中是私有字段，
                // 并且在构造时通过 INavigationService 绑定到 NavigateCommand。
                // 如果这些需要动态更新，MenuItemViewModel 内部需要提供公共属性或方法来处理。
                // 目前，我们假设如果这些变化，IMenuService 会返回一个新的 MenuItemViewModel 实例。
                // 如果需要更细粒度的更新，需要修改 MenuItemViewModel 的设计。
                // 这里我们只更新直接暴露的 ObservableProperty。
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
                Menus.Add(_mapper.Map<MenuBeanItemViewModel>(newDto));
            }
        }
    }



    // /// <summary>
    // /// 异步根据ID获取设备数据。
    // /// </summary>
    // /// <param name="id">设备ID。</param>
    // /// <returns>设备对象，如果不存在则为null。</returns>
    // public async Task<Device> GetDeviceByIdAsync(int id)
    // {
    //     return await _deviceRepository.GetByIdAsync(id);
    // }
    //
    // /// <summary>
    // /// 异步加载变量数据。
    // /// </summary>
    // /// <returns>表示异步操作的任务。</returns>
    // private async Task LoadVariables()
    // {
    //     Variables = await _varDataRepository.GetAllAsync();
    // }

    // /// <summary>
    // /// 异步更新变量数据。
    // /// </summary>
    // /// <param name="variable">要更新的变量数据。</param>
    // /// <returns>表示异步操作的任务。</returns>
    // public async Task UpdateVariableAsync(Variable variable)
    // {
    //     await _variableAppService.UpdateVariableAsync(_mapper.Map<VariableDto>(variable));
    // }

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
                if (existingItem.DataType != dto.DataType) existingItem.DataType = dto.DataType;
                if (existingItem.PollLevel != dto.PollLevel) existingItem.PollLevel = dto.PollLevel;
                if (existingItem.IsActive != dto.IsActive) existingItem.IsActive = dto.IsActive;
                if (existingItem.VariableTableId != dto.VariableTableId) existingItem.VariableTableId = dto.VariableTableId;
                if (existingItem.OpcUaNodeId != dto.OpcUaNodeId) existingItem.OpcUaNodeId = dto.OpcUaNodeId;
                if (existingItem.IsHistoryEnabled != dto.IsHistoryEnabled) existingItem.IsHistoryEnabled = dto.IsHistoryEnabled;
                if (existingItem.HistoryDeadband != dto.HistoryDeadband) existingItem.HistoryDeadband = dto.HistoryDeadband;
                if (existingItem.IsAlarmEnabled != dto.IsAlarmEnabled) existingItem.IsAlarmEnabled = dto.IsAlarmEnabled;
                if (existingItem.AlarmMinValue != dto.AlarmMinValue) existingItem.AlarmMinValue = dto.AlarmMinValue;
                if (existingItem.AlarmMaxValue != dto.AlarmMaxValue) existingItem.AlarmMaxValue = dto.AlarmMaxValue;
                if (existingItem.AlarmDeadband != dto.AlarmDeadband) existingItem.AlarmDeadband = dto.AlarmDeadband;
                if (existingItem.Protocol != dto.Protocol) existingItem.Protocol = dto.Protocol;
                if (existingItem.CSharpDataType != dto.CSharpDataType) existingItem.CSharpDataType = dto.CSharpDataType;
                if (existingItem.ConversionFormula != dto.ConversionFormula) existingItem.ConversionFormula = dto.ConversionFormula;
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
}