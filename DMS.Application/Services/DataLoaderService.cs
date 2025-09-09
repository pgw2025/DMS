using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using System.Collections.Concurrent;

namespace DMS.Application.Services;

/// <summary>
/// 数据加载服务，负责从数据源加载数据到内存中
/// </summary>
public class DataLoaderService : IDataLoaderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IDeviceAppService _deviceAppService;
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IVariableAppService _variableAppService;
    private readonly IMenuService _menuService;
    private readonly IMqttAppService _mqttAppService;
    private readonly INlogAppService _nlogAppService;

    public DataLoaderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuService menuService,
        IMqttAppService mqttAppService,
        INlogAppService nlogAppService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
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
    public async Task LoadAllDataToMemoryAsync(
        ConcurrentDictionary<int, DeviceDto> devices,
        ConcurrentDictionary<int, VariableTableDto> variableTables,
        ConcurrentDictionary<int, VariableDto> variables,
        ConcurrentDictionary<int, MenuBeanDto> menus,
        ConcurrentDictionary<int, MenuBeanDto> menuTrees,
        ConcurrentDictionary<int, MqttServerDto> mqttServers,
        ConcurrentDictionary<int, NlogDto> nlogs)
    {
        // 清空现有数据
        devices.Clear();
        variableTables.Clear();
        variables.Clear();
        menus.Clear();
        menuTrees.Clear();
        mqttServers.Clear();
        nlogs.Clear();

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
        var nlogDtos = await LoadAllNlogsAsync();

        // 获取变量MQTT别名
        var variableMqttAliases = await _repositoryManager.VariableMqttAliases.GetAllAsync();

        // 建立设备与变量表的关联
        foreach (var deviceDto in deviceDtos)
        {
            deviceDto.VariableTables = variableTableDtos
                                       .Where(vt => vt.DeviceId == deviceDto.Id)
                                       .ToList();

            // 将设备添加到安全字典
            devices.TryAdd(deviceDto.Id, deviceDto);
        }

        // 建立变量表与变量的关联
        foreach (var variableTableDto in variableTableDtos)
        {
            variableTableDto.Variables = variableDtos
                                         .Where(v => v.VariableTableId == variableTableDto.Id)
                                         .ToList();
            if (devices.TryGetValue(variableTableDto.DeviceId, out var deviceDto))
            {
                variableTableDto.Device = deviceDto;
            }

            // 将变量表添加到安全字典
            variableTables.TryAdd(variableTableDto.Id, variableTableDto);
        }

        // 加载MQTT服务器数据到内存
        foreach (var mqttServerDto in mqttServerDtos)
        {
            mqttServers.TryAdd(mqttServerDto.Id, mqttServerDto);
        }

        // 加载日志数据到内存
        foreach (var nlogDto in nlogDtos)
        {
            nlogs.TryAdd(nlogDto.Id, nlogDto);
        }

        // 将变量添加到安全字典
        foreach (var variableDto in variableDtos)
        {
            if (variableTables.TryGetValue(variableDto.VariableTableId, out var variableTableDto))
            {
                variableDto.VariableTable = variableTableDto;
            }

            variables.TryAdd(variableDto.Id, variableDto);
        }

        // 将菜单添加到安全字典
        foreach (var menuDto in menuDtos)
        {
            menus.TryAdd(menuDto.Id, menuDto);
        }
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
    public async Task<List<NlogDto>> LoadAllNlogsAsync()
    {
        return await _nlogAppService.GetAllLogsAsync();
    }
}