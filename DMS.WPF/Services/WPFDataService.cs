using System.Collections.ObjectModel;
using AutoMapper;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 主数据服务，用于管理所有子数据服务。
/// </summary>
public class WPFDataService : IWPFDataService
{
    private readonly IMapper _mapper;
    private readonly IAppDataCenterService _appDataCenterService;

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
    /// 日志数据服务。
    /// </summary>
    public ILogDataService LogDataService { get; }

    /// <summary>
    /// WPFDataService 构造函数。
    /// </summary>
    public WPFDataService(
        IMapper mapper,
        IAppDataCenterService appDataCenterService,
        IDeviceDataService deviceDataService,
        IVariableDataService variableDataService,
        IMenuDataService menuDataService,
        IMqttDataService mqttDataService,
        ILogDataService logDataService, IVariableTableDataService variableTableDataService)
    {
        _mapper = mapper;
        _appDataCenterService = appDataCenterService;
        DeviceDataService = deviceDataService;
        VariableDataService = variableDataService;
        MenuDataService = menuDataService;
        MqttDataService = mqttDataService;
        LogDataService = logDataService;
        VariableTableDataService = variableTableDataService;

    }
}
