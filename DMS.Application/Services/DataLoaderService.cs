using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using System.Collections.Concurrent;
using DMS.Application.Events;
using DMS.Application.Interfaces.Database;
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
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDeviceAppService _deviceAppService;
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IVariableAppService _variableAppService;
    private readonly IMenuAppService _menuService;
    private readonly IMqttAppService _mqttAppService;
    private readonly INlogAppService _nlogAppService;
    private readonly ITriggerManagementService _triggerManagementService; // 添加触发器管理服务
    private readonly IEventService _eventService; // 添加事件服务



    public const int LoadLogCount =100;
    public DataLoaderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IAppDataStorageService appDataStorageService,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuAppService menuService,
        IMqttAppService mqttAppService,
        INlogAppService nlogAppService,
        ITriggerManagementService triggerManagementService, // 添加触发器管理服务参数
        IEventService eventService) // 添加事件服务参数
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _appDataStorageService = appDataStorageService;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
        _menuService = menuService;
        _mqttAppService = mqttAppService;
        _nlogAppService = nlogAppService;
        _triggerManagementService = triggerManagementService; // 初始化触发器管理服务
        _eventService = eventService; // 初始化事件服务
    }


    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中
    /// </summary>
    public async Task LoadAllDataToMemoryAsync()
    {

        await LoadAllDevicesAsync();

        await LoadAllVariableTablesAsync();

        await LoadAllVariablesAsync();
        // 加载所有菜单
        await LoadAllMenusAsync();

        // 加载所有MQTT服务器
        await LoadAllMqttServersAsync();

        // 加载所有日志
        await LoadAllNlogsAsync(LoadLogCount);

        // 获取变量MQTT别名
        await LoadAllVariableMqttAliases();
        
        // 加载所有触发器
        await LoadAllTriggersAsync();

        _eventService.RaiseLoadDataCompleted(this, new DataLoadCompletedEventArgs(true, "数据加载成功"));
    }

    /// <summary>
    /// 异步加载所有触发器数据
    /// </summary>
    public async Task LoadAllTriggersAsync()
    {
        _appDataStorageService.Triggers.Clear();
        var triggers = await _triggerManagementService.GetAllTriggersAsync();
        // 加载触发器数据到内存
        foreach (var trigger in triggers)
        {
            _appDataStorageService.Triggers.TryAdd(trigger.Id, trigger);
        }
    }

    private async Task LoadAllVariableMqttAliases()
    {

        var variableMqttAliases = await _repositoryManager.VariableMqttAliases.GetAllAsync();
        foreach (var variableMqttAlias in variableMqttAliases)
        {
            _appDataStorageService.VariableMqttAliases.TryAdd(variableMqttAlias.Id, variableMqttAlias);
            if (_appDataStorageService.Variables.TryGetValue(variableMqttAlias.VariableId, out var variable))
            {
                variableMqttAlias.Variable = _mapper.Map<Variable>(variable);
                variable.MqttAliases?.Add(variableMqttAlias);
            }

            if (_appDataStorageService.MqttServers.TryGetValue(variableMqttAlias.MqttServerId, out var mqttServer))
            {
                variableMqttAlias.MqttServer = mqttServer;
                mqttServer.VariableAliases?.Add(variableMqttAlias);
            }
        }
    }

    /// <summary>
    /// 异步加载所有设备数据
    /// </summary>
    public async Task LoadAllDevicesAsync()
    {
        _appDataStorageService.Devices.Clear();
        var devices = await _repositoryManager.Devices.GetAllAsync();
        var devicesDtos = _mapper.Map<List<Device>>(devices);

        // 建立设备与变量表的关联
        foreach (var deviceDto in devicesDtos)
        {
            // 将设备添加到安全字典
            _appDataStorageService.Devices.TryAdd(deviceDto.Id, deviceDto);
        }
    }

    /// <summary>
    /// 异步加载所有变量表数据
    /// </summary>
    public async Task LoadAllVariableTablesAsync()
    {
        _appDataStorageService.VariableTables.Clear();
        var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
        var variableTableDtos = _mapper.Map<List<VariableTable>>(variableTables);
        // 建立变量表与变量的关联
        foreach (var variableTableDto in variableTableDtos)
        {
            if (_appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var deviceDto))
            {
                variableTableDto.Device = deviceDto;
                variableTableDto.Device.VariableTables.Add(variableTableDto);
            }

            // 将变量表添加到安全字典
            _appDataStorageService.VariableTables.TryAdd(variableTableDto.Id, variableTableDto);
        }
    }

    /// <summary>
    /// 异步加载所有变量数据
    /// </summary>
    public async Task LoadAllVariablesAsync()
    {
        _appDataStorageService.Variables.Clear();

        var variables = await _repositoryManager.Variables.GetAllAsync();
        var variableDtos = _mapper.Map<List<Variable>>(variables);
        // 将变量添加到安全字典
        foreach (var variableDto in variableDtos)
        {
            if (_appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId,
                                                                  out var variableTableDto))
            {
                variableDto.VariableTable = variableTableDto;
                variableDto.VariableTable.Variables.Add(variableDto);
            }

            _appDataStorageService.Variables.TryAdd(variableDto.Id, variableDto);
        }
    }

    /// <summary>
    /// 异步加载所有菜单数据
    /// </summary>
    public async Task LoadAllMenusAsync()
    {
        _appDataStorageService.Menus.Clear();
        _appDataStorageService.MenuTrees.Clear();
        var menus = await _repositoryManager.Menus.GetAllAsync();
        var menuDtos = _mapper.Map<List<MenuBeanDto>>(menus);
        // 将菜单添加到安全字典
        foreach (var menuDto in menuDtos)
        {
            _appDataStorageService.Menus.TryAdd(menuDto.Id, menuDto);
        }

    }

    /// <summary>
    /// 异步加载所有MQTT服务器数据
    /// </summary>
    public async Task LoadAllMqttServersAsync()
    {
        _appDataStorageService.MqttServers.Clear();
        var  mqttServers =await _mqttAppService.GetAllMqttServersAsync();
        // 加载MQTT服务器数据到内存
        foreach (var mqttServer in mqttServers)
        {
            _appDataStorageService.MqttServers.TryAdd(mqttServer.Id, mqttServer);
        }
    }

    /// <summary>
    /// 异步加载所有日志数据
    /// </summary>
    public async Task LoadAllNlogsAsync(int count)
    {
        _appDataStorageService.Nlogs.Clear();
        var nlogDtos =await _nlogAppService.GetLatestLogsAsync(count);
        // 加载日志数据到内存
        foreach (var nlogDto in nlogDtos)
        {
            _appDataStorageService.Nlogs.TryAdd(nlogDto.Id, nlogDto);
        }
        
    }
}