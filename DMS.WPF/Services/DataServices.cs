using System.Collections.Concurrent;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Data.Repositories;
using DMS.Core.Enums;
using DMS.Helper;
using DMS.Message;
using DMS.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DMS.ViewModels;
using SqlSugar;

namespace DMS.Services;

/// <summary>
/// 数据服务类，负责从数据库加载和管理各种数据，并提供数据变更通知。
/// 继承自ObservableRecipient，可以接收消息；实现IRecipient<LoadMessage>，处理加载消息。
/// </summary>
public partial class DataServices : ObservableRecipient, IRecipient<LoadMessage>
{
    private readonly IMapper _mapper;
    // 设备列表，使用ObservableProperty特性，当值改变时会自动触发属性变更通知。
    [ObservableProperty]
    private List<Device> _devices;

    // 变量表列表。
    [ObservableProperty]
    private List<VariableTable> _variableTables;

    // 变量数据列表。
    [ObservableProperty]
    private List<Variable> _variables;

    // 菜单树列表。
    [ObservableProperty]
    private List<MenuBean> menuTrees;

    // MQTT配置列表。
    [ObservableProperty]
    private List<Mqtt> _mqtts;


    public ConcurrentDictionary<int, Variable> AllVariables;

    // 设备数据仓库，用于设备数据的CRUD操作。
    private readonly DeviceRepository _deviceRepository;

    // 菜单数据仓库，用于菜单数据的CRUD操作。
    private readonly MenuRepository _menuRepository;

    // MQTT数据仓库，用于MQTT配置数据的CRUD操作。
    private readonly MqttRepository _mqttRepository;

    // 变量数据仓库，用于变量数据的CRUD操作。
    private readonly VarDataRepository _varDataRepository;

    // 设备列表变更事件，当设备列表数据更新时触发。
    public event Action<List<Device>> OnDeviceListChanged;

    // 菜单树列表变更事件，当菜单树数据更新时触发。
    public event Action<List<MenuBean>> OnMenuTreeListChanged;

    // MQTT列表变更事件，当MQTT配置数据更新时触发。
    public event Action<List<Mqtt>> OnMqttListChanged;

    // 设备IsActive状态变更事件，当单个设备的IsActive状态改变时触发。
    public event Action<Device, bool> OnDeviceIsActiveChanged;


    /// <summary>
    /// 当_mqtts属性值改变时触发的局部方法，用于调用OnMqttListChanged事件。
    /// </summary>
    /// <param name="mqtts">新的MQTT配置列表。</param>
    partial void OnMqttsChanged(List<Mqtt> mqtts)
    {
        OnMqttListChanged?.Invoke(mqtts);
    }

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
    public DataServices(IMapper mapper,DeviceRepository deviceRepository,MenuRepository menuRepository,MqttRepository mqttRepository,VarDataRepository varDataRepository)
    {
        _mapper = mapper;
        IsActive = true; // 激活消息接收器
        _deviceRepository = deviceRepository;
        _menuRepository = menuRepository;
        _mqttRepository = mqttRepository;
        _varDataRepository = varDataRepository;
        _variables = new List<Variable>();
        AllVariables = new ConcurrentDictionary<int, Variable>();
    }

    /// <summary>
    /// 接收加载消息，根据消息类型从数据库加载对应的数据。
    /// </summary>
    /// <param name="message">加载消息，包含要加载的数据类型。</param>
    /// <exception cref="ArgumentException">如果加载类型未知，可能会抛出此异常（尽管当前实现中未显式抛出）。</exception>
    public async void Receive(LoadMessage message)
    {
        try
        {
            switch (message.LoadType)
            {
                case LoadTypes.All: // 加载所有数据
                    await LoadDevices();
                    await LoadMenus();
                    await LoadMqtts();
                    break;
                case LoadTypes.Devices: // 仅加载设备数据
                    await LoadDevices();
                    break;
                case LoadTypes.Menu: // 仅加载菜单数据
                    await LoadMenus();
                    break;
                case LoadTypes.Mqtts: // 仅加载MQTT配置数据
                    await LoadMqtts();
                    break;
            }
        }
        catch (Exception e)
        {
            // 捕获加载数据时发生的异常，并通过通知和日志记录错误信息。
            NotificationHelper.ShowError($"加载数据出现了错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 异步加载设备数据。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    private async Task LoadDevices()
    {
        // 取消订阅旧设备的属性变更事件，防止内存泄漏
        if (Devices != null)
        {
            foreach (var device in Devices)
            {
                device.PropertyChanged -= Device_PropertyChanged;
            }
        }

        Devices = await _deviceRepository.GetAllAsync();

        // 订阅新设备的属性变更事件
        if (Devices != null)
        {
            foreach (var device in Devices)
            {
                device.PropertyChanged += Device_PropertyChanged;
            }

            var allVar = await _varDataRepository.GetAllAsync();
           foreach (var variable in allVar)
           {
               AllVariables.AddOrUpdate(variable.Id, variable, (key, old) => variable);
           }

        }

        OnDeviceListChanged?.Invoke(Devices);
    }

    private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Device.IsActive))
        {
            if (sender is Device device)
            {
                NlogHelper.Info($"设备 {device.Name} 的IsActive状态改变为 {device.IsActive}，触发设备IsActive状态变更事件。");
                OnDeviceIsActiveChanged?.Invoke(device, device.IsActive);
            }
        }
    }

    /// <summary>
    /// 异步加载菜单数据，并进行父级关联和排序。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    private async Task LoadMenus()
    {
        MenuTrees = await _menuRepository.GetMenuTreesAsync();
        foreach (MenuBean menu in MenuTrees)
        {
            MenuHelper.MenuAddParent(menu); // 为菜单添加父级引用
            DataServicesHelper.SortMenus(menu); // 排序菜单
        }

        OnMenuTreeListChanged?.Invoke(MenuTrees);
    }

    /// <summary>
    /// 异步获取所有MQTT配置。
    /// </summary>
    /// <returns>包含所有MQTT配置的列表。</returns>
    public async Task<List<Mqtt>> GetMqttsAsync()
    {
        var mqtts = await _mqttRepository.GetAllAsync();
        OnMqttListChanged?.Invoke(mqtts);
        return mqtts;
    }

    /// <summary>
    /// 异步加载MQTT配置数据。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    private async Task LoadMqtts()
    {
        Mqtts = await _mqttRepository.GetAllAsync();
    }


    /// <summary>
    /// 异步根据ID获取设备数据。
    /// </summary>
    /// <param name="id">设备ID。</param>
    /// <returns>设备对象，如果不存在则为null。</returns>
    public async Task<Device> GetDeviceByIdAsync(int id)
    {
        return await _deviceRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// 异步加载变量数据。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    private async Task LoadVariables()
    {
        Variables = await _varDataRepository.GetAllAsync();
    }

    /// <summary>
    /// 异步更新变量数据。
    /// </summary>
    /// <param name="variable">要更新的变量数据。</param>
    /// <returns>表示异步操作的任务。</returns>
    public async Task UpdateVariableAsync(Variable variable)
    {
        await _varDataRepository.UpdateAsync(variable);
    }

}