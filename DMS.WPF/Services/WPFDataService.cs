using System.Collections.ObjectModel;
using AutoMapper;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Services;

/// <summary>
/// 主数据服务，用于管理所有子数据服务。
/// </summary>
public class WPFDataService : IWPFDataService
{
    private readonly IMapper _mapper;
    private readonly IAppCenterService _appCenterService;

    /// <summary>
    /// 设备数据服务。
    /// </summary>
    public IDeviceDataService DeviceDataService { get; }

    /// <summary>
    /// 变量表数据服务。
    /// </summary>
    public IVariableTableDataService VariableTableDataService { get; }

    /// <summary>
    /// 变量数据服务。
    /// </summary>
    public IVariableDataService VariableDataService { get; }

    /// <summary>
    /// 菜单数据服务。
    /// </summary>
    public IMenuDataService MenuDataService { get; }

    /// <summary>
    /// MQTT数据服务。
    /// </summary>
    public IMqttDataService MqttDataService { get; }

    /// <summary>
    /// MQTT别名数据服务。
    /// </summary>
    public IMqttAliasDataService MqttAliasDataService { get; }

    /// <summary>
    /// 日志数据服务。
    /// </summary>
    public ILogDataService LogDataService { get; }

    /// <summary>
    /// 触发器数据服务。
    /// </summary>
    public ITriggerDataService TriggerDataService { get; }

    /// <summary>
    /// WPFDataService 构造函数。
    /// </summary>
    public WPFDataService(
        IMapper mapper,
        IAppCenterService appCenterService,
        IDeviceDataService deviceDataService,
        IVariableDataService variableDataService,
        IMenuDataService menuDataService,
        IMqttDataService mqttDataService,
        ILogDataService logDataService, 
        IVariableTableDataService variableTableDataService,
        ITriggerDataService triggerDataService,
        IMqttAliasDataService mqttAliasDataService)
    {
        _mapper = mapper;
        _appCenterService = appCenterService;
        DeviceDataService = deviceDataService;
        VariableDataService = variableDataService;
        MenuDataService = menuDataService;
        MqttDataService = mqttDataService;
        LogDataService = logDataService;
        VariableTableDataService = variableTableDataService;
        TriggerDataService = triggerDataService;
        MqttAliasDataService = mqttAliasDataService;
    }
}
