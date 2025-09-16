using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using System.Collections.Concurrent;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces.Database;
using DMS.Core.Models;

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
    private readonly IMenuService _menuService;
    private readonly IMqttAppService _mqttAppService;
    private readonly INlogAppService _nlogAppService;

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    public event EventHandler<DataLoadCompletedEventArgs> OnLoadDataCompleted;

    public const int LoadLogCount =100;
    public DataLoaderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IAppDataStorageService appDataStorageService,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuService menuService,
        IMqttAppService mqttAppService,
        INlogAppService nlogAppService)
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

        OnLoadDataCompleted?.Invoke(this, new DataLoadCompletedEventArgs(true, "数据加载成功"));
    }

    private async Task LoadAllVariableMqttAliases()
    {
       
        var variableMqttAliases = await _repositoryManager.VariableMqttAliases.GetAllAsync();
       var variableMqttAliasDtos = _mapper.Map<IEnumerable<VariableMqttAliasDto>>(variableMqttAliases);
       foreach (var variableMqttAliasDto in variableMqttAliasDtos)
       {
           _appDataStorageService.VariableMqttAliases.TryAdd(variableMqttAliasDto.Id, variableMqttAliasDto);
           if (_appDataStorageService.Variables.TryGetValue(variableMqttAliasDto.VariableId, out var variable))
           {
               variableMqttAliasDto.Variable = _mapper.Map<Variable>(variable);
               variable.MqttAliases?.Add(variableMqttAliasDto);
           }

           if (_appDataStorageService.MqttServers.TryGetValue(variableMqttAliasDto.MqttServerId, out var mqttServer))
           {
               variableMqttAliasDto.MqttServer = _mapper.Map<MqttServer>(mqttServer);
               variableMqttAliasDto.MqttServerName = variableMqttAliasDto.MqttServer.ServerName;
               mqttServer.VariableAliases?.Add(variableMqttAliasDto);
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
        var devicesDtos = _mapper.Map<List<DeviceDto>>(devices);

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
        var variableTableDtos = _mapper.Map<List<VariableTableDto>>(variableTables);
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
        var variableDtos = _mapper.Map<List<VariableDto>>(variables);
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
        var  mqttServerDtos =await _mqttAppService.GetAllMqttServersAsync();
        // 加载MQTT服务器数据到内存
        foreach (var mqttServerDto in mqttServerDtos)
        {
            _appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto);
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