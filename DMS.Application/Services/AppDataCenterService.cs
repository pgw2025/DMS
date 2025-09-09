using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DMS.Application.Services;

/// <summary>
/// 数据中心服务，负责管理所有的数据，包括设备、变量表、变量、菜单和日志。
/// 实现 <see cref="IAppDataCenterService"/> 接口。
/// </summary>
public class AppDataCenterService : IAppDataCenterService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IDataLoaderService _dataLoaderService;
    
    // 管理服务
    private readonly DeviceManagementService _deviceManagementService;
    private readonly VariableTableManagementService _variableTableManagementService;
    private readonly VariableManagementService _variableManagementService;
    private readonly MenuManagementService _menuManagementService;
    private readonly MqttManagementService _mqttManagementService;
    private readonly LogManagementService _logManagementService;

    /// <summary>
    /// 安全字典，用于存储所有设备数据
    /// </summary>
    public ConcurrentDictionary<int, DeviceDto> Devices { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有变量表数据
    /// </summary>
    public ConcurrentDictionary<int, VariableTableDto> VariableTables { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有变量数据
    /// </summary>
    public ConcurrentDictionary<int, VariableDto> Variables { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    public ConcurrentDictionary<int, MenuBeanDto> Menus { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    public ConcurrentDictionary<int, MenuBeanDto> MenuTrees { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有MQTT服务器数据
    /// </summary>
    public ConcurrentDictionary<int, MqttServerDto> MqttServers { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有日志数据
    /// </summary>
    public ConcurrentDictionary<int, NlogDto> Nlogs { get; } = new();

    #region 事件定义

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    public event EventHandler<DataLoadCompletedEventArgs> OnLoadDataCompleted;

    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> VariableTableChanged;

    /// <summary>
    /// 当变量数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableChangedEventArgs> VariableChanged;

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    public event EventHandler<MenuChangedEventArgs> MenuChanged;

    /// <summary>
    /// 当MQTT服务器数据发生变化时触发
    /// </summary>
    public event EventHandler<MqttServerChangedEventArgs> MqttServerChanged;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    public event EventHandler<NlogChangedEventArgs> NlogChanged;

    /// <summary>
    /// 当变量值发生变化时触发
    /// </summary>
    public event EventHandler<VariableValueChangedEventArgs> VariableValueChanged;

    #endregion

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和相关服务实例。
    /// </summary>
    /// <param name="repositoryManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="deviceAppService">设备应用服务实例。</param>
    /// <param name="variableTableAppService">变量表应用服务实例。</param>
    /// <param name="variableAppService">变量应用服务实例。</param>
    /// <param name="menuService">菜单服务实例。</param>
    /// <param name="mqttAppService">MQTT应用服务实例。</param>
    /// <param name="nlogAppService">Nlog应用服务实例。</param>
    /// <param name="dataLoaderService">数据加载服务实例。</param>
    public AppDataCenterService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuService menuService,
        IMqttAppService mqttAppService,
        INlogAppService nlogAppService,
        IDataLoaderService dataLoaderService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _dataLoaderService = dataLoaderService;
        
        // 初始化管理服务
        _deviceManagementService = new DeviceManagementService(deviceAppService, Devices);
        _variableTableManagementService = new VariableTableManagementService(variableTableAppService, VariableTables);
        _variableManagementService = new VariableManagementService(variableAppService, Variables);
        _menuManagementService = new MenuManagementService(menuService, Menus, MenuTrees);
        _mqttManagementService = new MqttManagementService(mqttAppService, MqttServers);
        _logManagementService = new LogManagementService(nlogAppService, Nlogs);
    }

    #region 设备管理

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    public async Task<DeviceDto> GetDeviceByIdAsync(int id)
    {
        return await _deviceManagementService.GetDeviceByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        return await _deviceManagementService.GetAllDevicesAsync();
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        return await _deviceManagementService.CreateDeviceWithDetailsAsync(dto);
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    public async Task<int> UpdateDeviceAsync(DeviceDto deviceDto)
    {
        return await _deviceManagementService.UpdateDeviceAsync(deviceDto);
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    public async Task<bool> DeleteDeviceByIdAsync(int deviceId)
    {
        return await _deviceManagementService.DeleteDeviceByIdAsync(deviceId);
    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    public async Task ToggleDeviceActiveStateAsync(int id)
    {
        await _deviceManagementService.ToggleDeviceActiveStateAsync(id);
    }

    /// <summary>
    /// 在内存中添加设备
    /// </summary>
    public void AddDeviceToMemory(DeviceDto deviceDto)
    {
        _deviceManagementService.AddDeviceToMemory(deviceDto, VariableTables, Variables);
    }

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    public void UpdateDeviceInMemory(DeviceDto deviceDto)
    {
        _deviceManagementService.UpdateDeviceInMemory(deviceDto);
    }

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    public void RemoveDeviceFromMemory(int deviceId)
    {
        _deviceManagementService.RemoveDeviceFromMemory(deviceId, VariableTables, Variables);
    }

    #endregion

    #region 变量表管理

    /// <summary>
    /// 异步根据ID获取变量表DTO。
    /// </summary>
    public async Task<VariableTableDto> GetVariableTableByIdAsync(int id)
    {
        return await _variableTableManagementService.GetVariableTableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量表DTO列表。
    /// </summary>
    public async Task<List<VariableTableDto>> GetAllVariableTablesAsync()
    {
        return await _variableTableManagementService.GetAllVariableTablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量表及其关联菜单（事务性操作）。
    /// </summary>
    public async Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto dto)
    {
        return await _variableTableManagementService.CreateVariableTableAsync(dto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量表。
    /// </summary>
    public async Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto)
    {
        return await _variableTableManagementService.UpdateVariableTableAsync(variableTableDto);
    }

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTableAsync(int id)
    {
        return await _variableTableManagementService.DeleteVariableTableAsync(id);
    }

    /// <summary>
    /// 在内存中添加变量表
    /// </summary>
    public void AddVariableTableToMemory(VariableTableDto variableTableDto)
    {
        _variableTableManagementService.AddVariableTableToMemory(variableTableDto, Devices);
    }

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    public void UpdateVariableTableInMemory(VariableTableDto variableTableDto)
    {
        _variableTableManagementService.UpdateVariableTableInMemory(variableTableDto, Devices);
    }

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    public void RemoveVariableTableFromMemory(int variableTableId)
    {
        _variableTableManagementService.RemoveVariableTableFromMemory(variableTableId, Devices);
    }

    #endregion

    #region 菜单管理

    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    public async Task<List<MenuBeanDto>> GetAllMenusAsync()
    {
        return await _menuManagementService.GetAllMenusAsync();
    }

    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    public async Task<MenuBeanDto> GetMenuByIdAsync(int id)
    {
        return await _menuManagementService.GetMenuByIdAsync(id);
    }

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    public async Task<int> CreateMenuAsync(MenuBeanDto menuDto)
    {
        return await _menuManagementService.CreateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    public async Task UpdateMenuAsync(MenuBeanDto menuDto)
    {
        await _menuManagementService.UpdateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    public async Task DeleteMenuAsync(int id)
    {
        await _menuManagementService.DeleteMenuAsync(id);
    }

    /// <summary>
    /// 在内存中添加菜单
    /// </summary>
    public void AddMenuToMemory(MenuBeanDto menuDto)
    {
        _menuManagementService.AddMenuToMemory(menuDto);
    }

    /// <summary>
    /// 在内存中更新菜单
    /// </summary>
    public void UpdateMenuInMemory(MenuBeanDto menuDto)
    {
        _menuManagementService.UpdateMenuInMemory(menuDto);
    }

    /// <summary>
    /// 在内存中删除菜单
    /// </summary>
    public void RemoveMenuFromMemory(int menuId)
    {
        _menuManagementService.RemoveMenuFromMemory(menuId);
    }

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    public List<MenuBeanDto> GetRootMenus()
    {
        return _menuManagementService.GetRootMenus();
    }

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    public List<MenuBeanDto> GetChildMenus(int parentId)
    {
        return _menuManagementService.GetChildMenus(parentId);
    }

    #endregion

    #region 变量管理

    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    public async Task<VariableDto> GetVariableByIdAsync(int id)
    {
        return await _variableManagementService.GetVariableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    public async Task<List<VariableDto>> GetAllVariablesAsync()
    {
        return await _variableManagementService.GetAllVariablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    public async Task<VariableDto> CreateVariableAsync(VariableDto variableDto)
    {
        return await _variableManagementService.CreateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        return await _variableManagementService.UpdateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        return await _variableManagementService.UpdateVariablesAsync(variableDtos);
    }

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        return await _variableManagementService.DeleteVariableAsync(id);
    }

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        return await _variableManagementService.DeleteVariablesAsync(ids);
    }

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    public void AddVariableToMemory(VariableDto variableDto)
    {
        _variableManagementService.AddVariableToMemory(variableDto, VariableTables);
    }

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    public void UpdateVariableInMemory(VariableDto variableDto)
    {
        _variableManagementService.UpdateVariableInMemory(variableDto, VariableTables);
    }

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    public void RemoveVariableFromMemory(int variableId)
    {
        _variableManagementService.RemoveVariableFromMemory(variableId, VariableTables);
    }

    #endregion

    #region MQTT服务器管理

    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    public async Task<MqttServerDto> GetMqttServerByIdAsync(int id)
    {
        return await _mqttManagementService.GetMqttServerByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    public async Task<List<MqttServerDto>> GetAllMqttServersAsync()
    {
        return await _mqttManagementService.GetAllMqttServersAsync();
    }

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    public async Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        return await _mqttManagementService.CreateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        await _mqttManagementService.UpdateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    public async Task DeleteMqttServerAsync(int id)
    {
        await _mqttManagementService.DeleteMqttServerAsync(id);
    }

    /// <summary>
    /// 在内存中添加MQTT服务器
    /// </summary>
    public void AddMqttServerToMemory(MqttServerDto mqttServerDto)
    {
        _mqttManagementService.AddMqttServerToMemory(mqttServerDto);
    }

    /// <summary>
    /// 在内存中更新MQTT服务器
    /// </summary>
    public void UpdateMqttServerInMemory(MqttServerDto mqttServerDto)
    {
        _mqttManagementService.UpdateMqttServerInMemory(mqttServerDto);
    }

    /// <summary>
    /// 在内存中删除MQTT服务器
    /// </summary>
    public void RemoveMqttServerFromMemory(int mqttServerId)
    {
        _mqttManagementService.RemoveMqttServerFromMemory(mqttServerId);
    }

    #endregion

    #region 数据加载和初始化

    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中。
    /// </summary>
    public async Task LoadAllDataToMemoryAsync()
    {
        try
        {
            // 委托给数据加载服务加载所有数据
            await _dataLoaderService.LoadAllDataToMemoryAsync(
                Devices,
                VariableTables,
                Variables,
                Menus,
                MenuTrees,
                MqttServers,
                Nlogs);

            // 构建菜单树
            _menuManagementService.BuildMenuTree();

            // 触发数据加载完成事件
            OnDataLoadCompleted(new DataLoadCompletedEventArgs(true, "数据加载完成"));
        }
        catch (Exception ex)
        {
            OnDataLoadCompleted(new DataLoadCompletedEventArgs(false, $"数据加载失败: {ex.Message}"));
            throw;
        }
    }

    /// <summary>
    /// 异步加载所有设备及其关联数据。
    /// </summary>
    public async Task<List<DeviceDto>> LoadAllDevicesAsync()
    {
        return await _dataLoaderService.LoadAllDevicesAsync();
    }

    /// <summary>
    /// 异步加载所有变量表及其关联数据。
    /// </summary>
    public async Task<List<VariableTableDto>> LoadAllVariableTablesAsync()
    {
        return await _dataLoaderService.LoadAllVariableTablesAsync();
    }

    /// <summary>
    /// 异步加载所有变量数据。
    /// </summary>
    public async Task<List<VariableDto>> LoadAllVariablesAsync()
    {
        return await _dataLoaderService.LoadAllVariablesAsync();
    }

    /// <summary>
    /// 异步加载所有菜单数据。
    /// </summary>
    public async Task<List<MenuBeanDto>> LoadAllMenusAsync()
    {
        return await _dataLoaderService.LoadAllMenusAsync();
    }

    /// <summary>
    /// 异步加载所有MQTT服务器数据。
    /// </summary>
    public async Task<List<MqttServerDto>> LoadAllMqttServersAsync()
    {
        return await _dataLoaderService.LoadAllMqttServersAsync();
    }
    
    /// <summary>
    /// 异步加载所有日志数据。
    /// </summary>
    public async Task<List<NlogDto>> LoadAllNlogsAsync()
    {
        return await _dataLoaderService.LoadAllNlogsAsync();
    }

    #endregion

    #region 事件触发方法

    /// <summary>
    /// 触发数据加载完成事件
    /// </summary>
    protected virtual void OnDataLoadCompleted(DataLoadCompletedEventArgs e)
    {
        OnLoadDataCompleted?.Invoke(this, e);
    }

    /// <summary>
    /// 触发设备变更事件
    /// </summary>
    protected virtual void OnDeviceChanged(DeviceChangedEventArgs e)
    {
        DeviceChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发变量表变更事件
    /// </summary>
    protected virtual void OnVariableTableChanged(VariableTableChangedEventArgs e)
    {
        VariableTableChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发变量变更事件
    /// </summary>
    protected virtual void OnVariableChanged(VariableChangedEventArgs e)
    {
        VariableChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发菜单变更事件
    /// </summary>
    protected virtual void OnMenuChanged(MenuChangedEventArgs e)
    {
        MenuChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发MQTT服务器变更事件
    /// </summary>
    protected virtual void OnMqttServerChanged(MqttServerChangedEventArgs e)
    {
        MqttServerChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 触发日志变更事件
    /// </summary>
    protected virtual void OnNlogChanged(NlogChangedEventArgs e)
    {
        NlogChanged?.Invoke(this, e);
    }


    /// <summary>
    /// 触发变量值变更事件
    /// </summary>
    public void OnVariableValueChanged(VariableValueChangedEventArgs e)
    {
        VariableValueChanged?.Invoke(this, e);
    }

    #endregion
    
    #region 日志管理

    /// <summary>
    /// 异步根据ID获取日志DTO。
    /// </summary>
    public async Task<NlogDto> GetNlogByIdAsync(int id)
    {
        return await _logManagementService.GetNlogByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有日志DTO列表。
    /// </summary>
    public async Task<List<NlogDto>> GetAllNlogsAsync()
    {
        return await _logManagementService.GetAllNlogsAsync();
    }

    /// <summary>
    /// 异步获取指定数量的最新日志DTO列表。
    /// </summary>
    public async Task<List<NlogDto>> GetLatestNlogsAsync(int count)
    {
        return await _logManagementService.GetLatestNlogsAsync(count);
    }

    /// <summary>
    /// 异步清空所有日志。
    /// </summary>
    public async Task ClearAllNlogsAsync()
    {
        await _logManagementService.ClearAllNlogsAsync();
    }

    /// <summary>
    /// 在内存中添加日志
    /// </summary>
    public void AddNlogToMemory(NlogDto nlogDto)
    {
        _logManagementService.AddNlogToMemory(nlogDto);
    }

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    public void UpdateNlogInMemory(NlogDto nlogDto)
    {
        _logManagementService.UpdateNlogInMemory(nlogDto);
    }

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    public void RemoveNlogFromMemory(int nlogId)
    {
        _logManagementService.RemoveNlogFromMemory(nlogId);
    }

    #endregion
}