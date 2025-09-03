using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
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

    #region 事件定义

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    public event EventHandler<DataLoadCompletedEventArgs> DataLoadCompleted;

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
    /// 当数据发生任何变化时触发
    /// </summary>
    public event EventHandler<DataChangedEventArgs> DataChanged;

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
    public DataCenterService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService,
        IMenuService menuService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
        _menuService = menuService;
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
        if (Devices.TryRemove(deviceId, out var deviceDto))
        {
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
            variableTableDto.Device = deviceDto;
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
        return Menus.Values.Where(m => m.ParentId == 0).ToList();
    }

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    public List<MenuBeanDto> GetChildMenus(int parentId)
    {
        return Menus.Values.Where(m => m.ParentId == parentId).ToList();
    }

    #endregion

    #region 事件触发方法

    /// <summary>
    /// 触发数据加载完成事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnDataLoadCompleted(DataLoadCompletedEventArgs e)
    {
        DataLoadCompleted?.Invoke(this, e);
        OnDataChanged(new DataChangedEventArgs(DataChangeType.Loaded));
    }

    /// <summary>
    /// 触发设备变更事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnDeviceChanged(DeviceChangedEventArgs e)
    {
        DeviceChanged?.Invoke(this, e);
        OnDataChanged(new DataChangedEventArgs(e.ChangeType));
    }

    /// <summary>
    /// 触发变量表变更事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnVariableTableChanged(VariableTableChangedEventArgs e)
    {
        VariableTableChanged?.Invoke(this, e);
        OnDataChanged(new DataChangedEventArgs(e.ChangeType));
    }

    /// <summary>
    /// 触发变量变更事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnVariableChanged(VariableChangedEventArgs e)
    {
        VariableChanged?.Invoke(this, e);
        OnDataChanged(new DataChangedEventArgs(e.ChangeType));
    }

    /// <summary>
    /// 触发菜单变更事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnMenuChanged(MenuChangedEventArgs e)
    {
        MenuChanged?.Invoke(this, e);
        OnDataChanged(new DataChangedEventArgs(e.ChangeType));
    }

    /// <summary>
    /// 触发数据变更事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnDataChanged(DataChangedEventArgs e)
    {
        DataChanged?.Invoke(this, e);
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
                
                // 将变量表添加到安全字典
                VariableTables.TryAdd(variableTableDto.Id, variableTableDto);
            }

            // 将变量添加到安全字典
            foreach (var variableDto in variableDtos)
            {
                Variables.TryAdd(variableDto.Id, variableDto);
            }

            // 将菜单添加到安全字典
            foreach (var menuDto in menuDtos)
            {
                Menus.TryAdd(menuDto.Id, menuDto);
            }

            // 触发数据加载完成事件
            OnDataLoadCompleted(new DataLoadCompletedEventArgs(
                deviceDtos, 
                variableTableDtos, 
                variableDtos, 
                true));
        }
        catch (Exception ex)
        {
            // 触发数据加载失败事件
            OnDataLoadCompleted(new DataLoadCompletedEventArgs(
                new List<DeviceDto>(), 
                new List<VariableTableDto>(), 
                new List<VariableDto>(), 
                false, 
                ex.Message));
            throw new ApplicationException($"加载所有数据到内存时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步加载所有菜单数据。
    /// </summary>
    public async Task<List<MenuBeanDto>> LoadAllMenusAsync()
    {
        try
        {
            // 获取所有菜单
            var menus = await _repositoryManager.Menus.GetAllAsync();
            return _mapper.Map<List<MenuBeanDto>>(menus);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"加载所有菜单数据时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步加载所有设备及其关联数据。
    /// </summary>
    public async Task<List<DeviceDto>> LoadAllDevicesAsync()
    {
        try
        {
            // 获取所有设备
            var devices = await _repositoryManager.Devices.GetAllAsync();
            var deviceDtos = _mapper.Map<List<DeviceDto>>(devices);
            
            // 为每个设备加载关联的变量表和变量
            foreach (var deviceDto in deviceDtos)
            {
                // 获取设备的所有变量表
                var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
                var deviceVariableTables = variableTables.Where(vt => vt.DeviceId == deviceDto.Id).ToList();
                deviceDto.VariableTables = _mapper.Map<List<VariableTableDto>>(deviceVariableTables);
                
                // 为每个变量表加载关联的变量
                foreach (var variableTableDto in deviceDto.VariableTables)
                {
                    var variables = await _repositoryManager.Variables.GetAllAsync();
                    var tableVariables = variables.Where(v => v.VariableTableId == variableTableDto.Id).ToList();
                    variableTableDto.Variables = _mapper.Map<List<VariableDto>>(tableVariables);
                }
            }
            
            return deviceDtos;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"加载所有设备数据时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步加载所有变量表及其关联数据。
    /// </summary>
    public async Task<List<VariableTableDto>> LoadAllVariableTablesAsync()
    {
        try
        {
            // 获取所有变量表
            var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
            var variableTableDtos = _mapper.Map<List<VariableTableDto>>(variableTables);
            
            // 为每个变量表加载关联的变量
            foreach (var variableTableDto in variableTableDtos)
            {
                var variables = await _repositoryManager.Variables.GetAllAsync();
                var tableVariables = variables.Where(v => v.VariableTableId == variableTableDto.Id).ToList();
                variableTableDto.Variables = _mapper.Map<List<VariableDto>>(tableVariables);
            }
            
            return variableTableDtos;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"加载所有变量表数据时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步加载所有变量数据。
    /// </summary>
    public async Task<List<VariableDto>> LoadAllVariablesAsync()
    {
        try
        {
            // 获取所有变量
            var variables = await _repositoryManager.Variables.GetAllAsync();
            return _mapper.Map<List<VariableDto>>(variables);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"加载所有变量数据时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    #endregion
}