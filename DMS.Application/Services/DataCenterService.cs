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
/// 数据中心服务，负责管理所有的数据，包括设备、变量表、变量和菜单。
/// 实现 <see cref="IDataCenterService"/> 接口。
/// </summary>
public class DataCenterService : IDataCenterService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IDeviceAppService _deviceAppService;
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IVariableAppService _variableAppService;
    private readonly IMenuService _menuService;
    private readonly IMqttAppService _mqttAppService;

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
    public DataCenterService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuService menuService,
        IMqttAppService mqttAppService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
        _menuService = menuService;
        _mqttAppService = mqttAppService;
    }

    #region 设备管理

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    public async Task<DeviceDto> GetDeviceByIdAsync(int id)
    {
        return await _deviceAppService.GetDeviceByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        return await _deviceAppService.GetAllDevicesAsync();
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        return await _deviceAppService.CreateDeviceWithDetailsAsync(dto);
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    public async Task<int> UpdateDeviceAsync(DeviceDto deviceDto)
    {
        return await _deviceAppService.UpdateDeviceAsync(deviceDto);
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    public async Task<bool> DeleteDeviceByIdAsync(int deviceId)
    {
        return await _deviceAppService.DeleteDeviceByIdAsync(deviceId);
    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    public async Task ToggleDeviceActiveStateAsync(int id)
    {
        await _deviceAppService.ToggleDeviceActiveStateAsync(id);
    }

    /// <summary>
    /// 在内存中添加设备
    /// </summary>
    public void AddDeviceToMemory(DeviceDto deviceDto)
    {
        if (Devices.TryAdd(deviceDto.Id, deviceDto))
        {
            OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Added, deviceDto));
        }
    }

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    public void UpdateDeviceInMemory(DeviceDto deviceDto)
    {
        Devices.AddOrUpdate(deviceDto.Id, deviceDto, (key, oldValue) => deviceDto);
        OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Updated, deviceDto));
    }

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    public void RemoveDeviceFromMemory(int deviceId)
    {
        if (Devices.TryGetValue(deviceId, out var deviceDto))
        {
            foreach (var variableTable in deviceDto.VariableTables)
            {
                foreach (var variable in variableTable.Variables)
                {
                    Variables.TryRemove(variable.Id, out _);
                }

                VariableTables.TryRemove(variableTable.Id, out _);
            }

            Devices.TryRemove(deviceId, out _);

            OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Deleted, deviceDto));
        }
    }

    #endregion

    #region 变量表管理

    /// <summary>
    /// 异步根据ID获取变量表DTO。
    /// </summary>
    public async Task<VariableTableDto> GetVariableTableByIdAsync(int id)
    {
        return await _variableTableAppService.GetVariableTableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量表DTO列表。
    /// </summary>
    public async Task<List<VariableTableDto>> GetAllVariableTablesAsync()
    {
        return await _variableTableAppService.GetAllVariableTablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量表及其关联菜单（事务性操作）。
    /// </summary>
    public async Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto dto)
    {
        return await _variableTableAppService.CreateVariableTableAsync(dto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量表。
    /// </summary>
    public async Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto)
    {
        return await _variableTableAppService.UpdateVariableTableAsync(variableTableDto);
    }

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTableAsync(int id)
    {
        return await _variableTableAppService.DeleteVariableTableAsync(id);
    }

    /// <summary>
    /// 在内存中添加变量表
    /// </summary>
    public void AddVariableTableToMemory(VariableTableDto variableTableDto)
    {
        DeviceDto deviceDto = null;
        if (Devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
            device.VariableTables.Add(variableTableDto);
            variableTableDto.Device = device;
        }

        if (VariableTables.TryAdd(variableTableDto.Id, variableTableDto))
        {
            OnVariableTableChanged(new VariableTableChangedEventArgs(
                                       DataChangeType.Added,
                                       variableTableDto,
                                       deviceDto));
        }
    }

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    public void UpdateVariableTableInMemory(VariableTableDto variableTableDto)
    {
        DeviceDto deviceDto = null;
        if (Devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
        }

        VariableTables.AddOrUpdate(variableTableDto.Id, variableTableDto, (key, oldValue) => variableTableDto);
        OnVariableTableChanged(new VariableTableChangedEventArgs(
                                   DataChangeType.Updated,
                                   variableTableDto,
                                   deviceDto));
    }

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    public void RemoveVariableTableFromMemory(int variableTableId)
    {
        if (VariableTables.TryRemove(variableTableId, out var variableTableDto))
        {
            DeviceDto deviceDto = null;
            if (variableTableDto != null && Devices.TryGetValue(variableTableDto.DeviceId, out var device))
            {
                deviceDto = device;
                device.VariableTables.Remove(variableTableDto);
            }

            OnVariableTableChanged(new VariableTableChangedEventArgs(
                                       DataChangeType.Deleted,
                                       variableTableDto,
                                       deviceDto));
        }
    }

    #endregion

    #region 菜单管理

    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    public async Task<List<MenuBeanDto>> GetAllMenusAsync()
    {
        return await _menuService.GetAllMenusAsync();
    }

    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    public async Task<MenuBeanDto> GetMenuByIdAsync(int id)
    {
        return await _menuService.GetMenuByIdAsync(id);
    }

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    public async Task<int> CreateMenuAsync(MenuBeanDto menuDto)
    {
        return await _menuService.CreateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    public async Task UpdateMenuAsync(MenuBeanDto menuDto)
    {
        await _menuService.UpdateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    public async Task DeleteMenuAsync(int id)
    {
        await _menuService.DeleteMenuAsync(id);
    }

    /// <summary>
    /// 在内存中添加菜单
    /// </summary>
    public void AddMenuToMemory(MenuBeanDto menuDto)
    {
        if (Menus.TryAdd(menuDto.Id, menuDto))
        {
            MenuBeanDto parentMenu = null;
            if (menuDto.ParentId > 0 && Menus.TryGetValue(menuDto.ParentId, out var parent))
            {
                parentMenu = parent;
                parent.Children.Add(menuDto);
            }

            OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Added, menuDto, parentMenu));
        }
    }

    /// <summary>
    /// 在内存中更新菜单
    /// </summary>
    public void UpdateMenuInMemory(MenuBeanDto menuDto)
    {
        Menus.AddOrUpdate(menuDto.Id, menuDto, (key, oldValue) => menuDto);

        MenuBeanDto parentMenu = null;
        if (menuDto.ParentId > 0 && Menus.TryGetValue(menuDto.ParentId, out var parent))
        {
            parentMenu = parent;
        }

        OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Updated, menuDto, parentMenu));
    }

    /// <summary>
    /// 在内存中删除菜单
    /// </summary>
    public void RemoveMenuFromMemory(int menuId)
    {
        if (Menus.TryRemove(menuId, out var menuDto))
        {
            MenuBeanDto parentMenu = null;
            if (menuDto.ParentId > 0 && Menus.TryGetValue(menuDto.ParentId, out var parent))
            {
                parentMenu = parent;
            }

            OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Deleted, menuDto, parentMenu));
        }
    }

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    public List<MenuBeanDto> GetRootMenus()
    {
        return Menus.Values.Where(m => m.ParentId == 0)
                    .ToList();
    }

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    public List<MenuBeanDto> GetChildMenus(int parentId)
    {
        return Menus.Values.Where(m => m.ParentId == parentId)
                    .ToList();
    }

    #endregion

    #region 变量管理

    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    public async Task<VariableDto> GetVariableByIdAsync(int id)
    {
        return await _variableAppService.GetVariableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    public async Task<List<VariableDto>> GetAllVariablesAsync()
    {
        return await _variableAppService.GetAllVariablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    public async Task<VariableDto> CreateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.CreateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.UpdateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        return await _variableAppService.UpdateVariablesAsync(variableDtos);
    }

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        return await _variableAppService.DeleteVariableAsync(id);
    }

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        return await _variableAppService.DeleteVariablesAsync(ids);
    }

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    public void AddVariableToMemory(VariableDto variableDto)
    {
        VariableTableDto variableTableDto = null;
        if (VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
            variableDto.VariableTable = variableTableDto;
            variableTable.Variables.Add(variableDto);
        }

        if (Variables.TryAdd(variableDto.Id, variableDto))
        {
            OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Added, variableDto, variableTableDto));
        }
    }

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    public void UpdateVariableInMemory(VariableDto variableDto)
    {
        VariableTableDto variableTableDto = null;
        if (VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
        }

        Variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
        OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Updated, variableDto, variableTableDto));
    }

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    public void RemoveVariableFromMemory(int variableId)
    {
        if (Variables.TryRemove(variableId, out var variableDto))
        {
            VariableTableDto variableTableDto = null;
            if (variableDto != null && VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
            {
                variableTableDto = variableTable;
                variableTable.Variables.Remove(variableDto);
            }

            OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Deleted, variableDto, variableTableDto));
        }
    }

    #endregion

    #region MQTT服务器管理

    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    public async Task<MqttServerDto> GetMqttServerByIdAsync(int id)
    {
        return await _mqttAppService.GetMqttServerByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    public async Task<List<MqttServerDto>> GetAllMqttServersAsync()
    {
        return await _mqttAppService.GetAllMqttServersAsync();
    }

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    public async Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        return await _mqttAppService.CreateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        await _mqttAppService.UpdateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    public async Task DeleteMqttServerAsync(int id)
    {
        await _mqttAppService.DeleteMqttServerAsync(id);
    }

    /// <summary>
    /// 在内存中添加MQTT服务器
    /// </summary>
    public void AddMqttServerToMemory(MqttServerDto mqttServerDto)
    {
        if (MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto))
        {
            OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Added, mqttServerDto));
        }
    }

    /// <summary>
    /// 在内存中更新MQTT服务器
    /// </summary>
    public void UpdateMqttServerInMemory(MqttServerDto mqttServerDto)
    {
        MqttServers.AddOrUpdate(mqttServerDto.Id, mqttServerDto, (key, oldValue) => mqttServerDto);
        OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Updated, mqttServerDto));
    }

    /// <summary>
    /// 在内存中删除MQTT服务器
    /// </summary>
    public void RemoveMqttServerFromMemory(int mqttServerId)
    {
        if (MqttServers.TryRemove(mqttServerId, out var mqttServerDto))
        {
            OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Deleted, mqttServerDto));
        }
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
            // 清空现有数据
            Devices.Clear();
            VariableTables.Clear();
            Variables.Clear();
            Menus.Clear();
            MenuTrees.Clear();
            MqttServers.Clear();

            // 加载所有设备
            var devices = await _repositoryManager.Devices.GetAllAsync();
            var deviceDtos = _mapper.Map<List<DeviceDto>>(devices);

            // 加载所有变量表
            var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
            var variableTableDtos = _mapper.Map<List<VariableTableDto>>(variableTables);

            // 加载所有变量
            var variables = await _repositoryManager.Variables.GetAllAsync();
            var variableDtos = _mapper.Map<List<VariableDto>>(variables);

            // 加载所有菜单
            var menus = await _repositoryManager.Menus.GetAllAsync();
            var menuDtos = _mapper.Map<List<MenuBeanDto>>(menus);

            var mqttServers = await LoadAllMqttServersAsync();

            var variableMqttAliases = await _repositoryManager.VariableMqttAliases.GetAllAsync();

            // 建立设备与变量表的关联
            foreach (var deviceDto in deviceDtos)
            {
                deviceDto.VariableTables = variableTableDtos
                                           .Where(vt => vt.DeviceId == deviceDto.Id)
                                           .ToList();

                // 将设备添加到安全字典
                Devices.TryAdd(deviceDto.Id, deviceDto);
            }

            // 建立变量表与变量的关联
            foreach (var variableTableDto in variableTableDtos)
            {
                variableTableDto.Variables = variableDtos
                                             .Where(v => v.VariableTableId == variableTableDto.Id)
                                             .ToList();
                if (Devices.TryGetValue(variableTableDto.DeviceId, out var deviceDto))
                {
                    variableTableDto.Device = deviceDto;
                }

                // 将变量表添加到安全字典
                VariableTables.TryAdd(variableTableDto.Id, variableTableDto);
            }
            
            // 加载MQTT服务器数据到内存
            foreach (var mqttServer in mqttServers)
            {
                MqttServers.TryAdd(mqttServer.Id, mqttServer);
            }

            // 将变量添加到安全字典
            foreach (var variableDto in variableDtos)
            {
                if (VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
                {
                    variableDto.VariableTable = variableTable;
                }

               // var alises= variableMqttAliases.FirstOrDefault(vm => vm.VariableId == variableDto.Id);
               // if (alises != null)
               // {
               //
               //     var variableMqttAliasDto = _mapper.Map<VariableMqttAliasDto>(alises);
               //     variableMqttAliasDto.Variable = _mapper.Map<Variable>(variableDto) ;
               //     if (MqttServers.TryGetValue(variableMqttAliasDto.MqttServerId, out var mqttServerDto))
               //     {
               //         variableMqttAliasDto.MqttServer = _mapper.Map<MqttServer>(mqttServerDto) ;
               //         variableMqttAliasDto.MqttServerName = variableMqttAliasDto.MqttServer.ServerName;
               //     }
               //     
               //     variableDto.MqttAliases.Add(variableMqttAliasDto);
               // }

                Variables.TryAdd(variableDto.Id, variableDto);
            }

            // 将菜单添加到安全字典
            foreach (var menuDto in menuDtos)
            {
                Menus.TryAdd(menuDto.Id, menuDto);
            }

            

            // 构建菜单树
            BuildMenuTree();

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
        return await _deviceAppService.GetAllDevicesAsync();
    }

    /// <summary>
    /// 异步加载所有变量表及其关联数据。
    /// </summary>
    public async Task<List<VariableTableDto>> LoadAllVariableTablesAsync()
    {
        return await _variableTableAppService.GetAllVariableTablesAsync();
    }

    /// <summary>
    /// 异步加载所有变量数据。
    /// </summary>
    public async Task<List<VariableDto>> LoadAllVariablesAsync()
    {
        return await _variableAppService.GetAllVariablesAsync();
    }

    /// <summary>
    /// 异步加载所有菜单数据。
    /// </summary>
    public async Task<List<MenuBeanDto>> LoadAllMenusAsync()
    {
        return await _menuService.GetAllMenusAsync();
    }

    /// <summary>
    /// 异步加载所有MQTT服务器数据。
    /// </summary>
    public async Task<List<MqttServerDto>> LoadAllMqttServersAsync()
    {
        return await _mqttAppService.GetAllMqttServersAsync();
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
    /// 触发变量值变更事件
    /// </summary>
    public void OnVariableValueChanged(VariableValueChangedEventArgs e)
    {
        VariableValueChanged?.Invoke(this, e);
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    private void BuildMenuTree()
    {
        // 清空现有菜单树
        MenuTrees.Clear();

        // 获取所有根菜单
        var rootMenus = GetRootMenus();

        // 将根菜单添加到菜单树中
        foreach (var rootMenu in rootMenus)
        {
            MenuTrees.TryAdd(rootMenu.Id, rootMenu);
        }
    }

    #endregion
}