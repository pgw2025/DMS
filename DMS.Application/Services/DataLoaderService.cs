using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using System.Collections.Concurrent;
using DMS.Application.DTOs.Events;

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
        // 清空现有数据
        _appDataStorageService.Devices.Clear();
        _appDataStorageService.VariableTables.Clear();
        _appDataStorageService.Variables.Clear();
        _appDataStorageService.Menus.Clear();
        _appDataStorageService.MenuTrees.Clear();
        _appDataStorageService.MqttServers.Clear();
        _appDataStorageService.Nlogs.Clear();

        // 加载所有设备
        var deviceDtos = await LoadAllDevicesAsync();

        // 加载所有变量表
        var variableTableDtos = await LoadAllVariableTablesAsync();

        // 加载所有变量
        var variableDtos = await LoadAllVariablesAsync();

        // 加载所有菜单
        var menuDtos = await LoadAllMenusAsync();

        // 加载所有MQTT服务器
        var mqttServerDtos = await LoadAllMqttServersAsync();

        // 加载所有日志
        var nlogDtos = await LoadAllNlogsAsync(100);

        // 获取变量MQTT别名
        var variableMqttAliases = await _repositoryManager.VariableMqttAliases.GetAllAsync();

        // 建立设备与变量表的关联
        foreach (var deviceDto in deviceDtos)
        {
            deviceDto.VariableTables = variableTableDtos
                                       .Where(vt => vt.DeviceId == deviceDto.Id)
                                       .ToList();

            // 将设备添加到安全字典
            _appDataStorageService.Devices.TryAdd(deviceDto.Id, deviceDto);
        }

        // 建立变量表与变量的关联
        foreach (var variableTableDto in variableTableDtos)
        {
            variableTableDto.Variables = variableDtos
                                         .Where(v => v.VariableTableId == variableTableDto.Id)
                                         .ToList();
            if (_appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var deviceDto))
            {
                variableTableDto.Device = deviceDto;
            }

            // 将变量表添加到安全字典
            _appDataStorageService.VariableTables.TryAdd(variableTableDto.Id, variableTableDto);
        }

        // 加载MQTT服务器数据到内存
        foreach (var mqttServerDto in mqttServerDtos)
        {
            _appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto);
        }

        // 加载日志数据到内存
        foreach (var nlogDto in nlogDtos)
        {
            _appDataStorageService.Nlogs.TryAdd(nlogDto.Id, nlogDto);
        }

        // 将变量添加到安全字典
        foreach (var variableDto in variableDtos)
        {
            if (_appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTableDto))
            {
                variableDto.VariableTable = variableTableDto;
            }

            _appDataStorageService.Variables.TryAdd(variableDto.Id, variableDto);
        }

        // 将菜单添加到安全字典
        foreach (var menuDto in menuDtos)
        {
            _appDataStorageService.Menus.TryAdd(menuDto.Id, menuDto);
        }
        
        OnLoadDataCompleted?.Invoke(this,new DataLoadCompletedEventArgs(true,"数据加载成功"));
    }

    /// <summary>
    /// 异步加载所有设备数据
    /// </summary>
    public async Task<List<DeviceDto>> LoadAllDevicesAsync()
    {
        var devices = await _repositoryManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }

    /// <summary>
    /// 异步加载所有变量表数据
    /// </summary>
    public async Task<List<VariableTableDto>> LoadAllVariableTablesAsync()
    {
        var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
        return _mapper.Map<List<VariableTableDto>>(variableTables);
    }

    /// <summary>
    /// 异步加载所有变量数据
    /// </summary>
    public async Task<List<VariableDto>> LoadAllVariablesAsync()
    {
        var variables = await _repositoryManager.Variables.GetAllAsync();
        return _mapper.Map<List<VariableDto>>(variables);
    }

    /// <summary>
    /// 异步加载所有菜单数据
    /// </summary>
    public async Task<List<MenuBeanDto>> LoadAllMenusAsync()
    {
        var menus = await _repositoryManager.Menus.GetAllAsync();
        return _mapper.Map<List<MenuBeanDto>>(menus);
    }

    /// <summary>
    /// 异步加载所有MQTT服务器数据
    /// </summary>
    public async Task<List<MqttServerDto>> LoadAllMqttServersAsync()
    {
        return await _mqttAppService.GetAllMqttServersAsync();
    }

    /// <summary>
    /// 异步加载所有日志数据
    /// </summary>
    public async Task<List<NlogDto>> LoadAllNlogsAsync(int count)
    {
        return await _nlogAppService.GetLatestLogsAsync(count);
    }
}