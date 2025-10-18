using DMS.Application.Interfaces;
using System.Collections.Concurrent;
using DMS.Application.Events;
using DMS.Application.Interfaces.Management;
using DMS.Application.Services.Management;
using DMS.Core.Models;
using DMS.Core.Models.Triggers;
using DMS.Application.Services.Triggers;

namespace DMS.Application.Services;

/// <summary>
/// 数据加载服务，负责从数据源加载数据到内存中
/// </summary>
public class DataLoaderService : IDataLoaderService
{
    private readonly IMqttManagementService _mqttManagementService;
    private readonly IMqttAliasManagementService _mqttAliasManagementService;
    private readonly ITriggerManagementService _triggerManagementService; // 添加触发器管理服务
    private readonly IEventService _eventService; // 添加事件服务
    private readonly IDeviceManagementService _deviceManagementService; // 添加设备管理服务
    private readonly IVariableTableManagementService _variableTableManagementService; // 添加变量表管理服务
    private readonly IVariableManagementService _variableManagementService; // 添加变量管理服务
    private readonly IMenuManagementService _menuManagementService; // 添加菜单管理服务
    private readonly ILogManagementService _logManagementService; // 添加日志管理服务



    public const int LoadLogCount =100;
    public DataLoaderService(IMqttManagementService mqttManagementService,
                             IMqttAliasManagementService mqttAliasManagementService,
                             ITriggerManagementService triggerManagementService, // 添加触发器管理服务参数
                             IEventService eventService, // 添加事件服务参数
                             IDeviceManagementService deviceManagementService, // 添加设备管理服务参数
                             IVariableTableManagementService variableTableManagementService, // 添加变量表管理服务参数
                             IVariableManagementService variableManagementService, // 添加变量管理服务参数
                             IMenuManagementService menuManagementService, // 添加菜单管理服务参数
                             ILogManagementService logManagementService) // 添加日志管理服务参数
    {
        _mqttManagementService = mqttManagementService;
        _mqttAliasManagementService = mqttAliasManagementService;
        _triggerManagementService = triggerManagementService; // 初始化触发器管理服务
        _eventService = eventService; // 初始化事件服务
        _deviceManagementService = deviceManagementService; // 初始化设备管理服务
        _variableTableManagementService = variableTableManagementService; // 初始化变量表管理服务
        _variableManagementService = variableManagementService; // 初始化变量管理服务
        _menuManagementService = menuManagementService; // 初始化菜单管理服务
        _logManagementService = logManagementService; // 初始化日志管理服务
    }


    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中
    /// </summary>
    public async Task LoadAllDataToMemoryAsync()
    {

        await _deviceManagementService.LoadAllDevicesAsync();

        await _variableTableManagementService.LoadAllVariableTablesAsync();

        await _variableManagementService.LoadAllVariablesAsync();
        // 加载所有菜单
        await _menuManagementService.LoadAllMenusAsync();

        // 加载所有MQTT服务器
        await _mqttManagementService.LoadAllMqttServersAsync();

        // 加载所有日志
        await _logManagementService.LoadAllNlogsAsync(LoadLogCount);

        // 获取变量MQTT别名
        await _mqttAliasManagementService.LoadAllMqttAliasAsync();
        
        // 加载所有触发器
        await _triggerManagementService.LoadAllTriggersAsync();

        _eventService.RaiseLoadDataCompleted(this, new DataLoadCompletedEventArgs(true, "数据加载成功"));
    }

}