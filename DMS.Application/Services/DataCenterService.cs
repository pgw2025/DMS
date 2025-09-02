using AutoMapper;
using DMS.Application.DTOs;
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
/// 数据中心服务，负责管理所有的数据，包括设备、变量表和变量。
/// 实现 <see cref="IDataCenterService"/> 接口。
/// </summary>
public class DataCenterService : IDataCenterService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IDeviceAppService _deviceAppService;
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IVariableAppService _variableAppService;

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
    /// 构造函数，通过依赖注入获取仓储管理器和相关服务实例。
    /// </summary>
    /// <param name="repositoryManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="deviceAppService">设备应用服务实例。</param>
    /// <param name="variableTableAppService">变量表应用服务实例。</param>
    /// <param name="variableAppService">变量应用服务实例。</param>
    public DataCenterService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDeviceAppService deviceAppService,
        IVariableTableAppService variableTableAppService,
        IVariableAppService variableAppService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _deviceAppService = deviceAppService;
        _variableTableAppService = variableTableAppService;
        _variableAppService = variableAppService;
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
        Devices.TryAdd(deviceDto.Id, deviceDto);
    }

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    public void UpdateDeviceInMemory(DeviceDto deviceDto)
    {
        Devices.AddOrUpdate(deviceDto.Id, deviceDto, (key, oldValue) => deviceDto);
    }

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    public void RemoveDeviceFromMemory(int deviceId)
    {
        Devices.TryRemove(deviceId, out _);
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
        VariableTables.TryAdd(variableTableDto.Id, variableTableDto);
    }

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    public void UpdateVariableTableInMemory(VariableTableDto variableTableDto)
    {
        VariableTables.AddOrUpdate(variableTableDto.Id, variableTableDto, (key, oldValue) => variableTableDto);
    }

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    public void RemoveVariableTableFromMemory(int variableTableId)
    {
        VariableTables.TryRemove(variableTableId, out _);
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
    /// 异步创建一个新变量（事务性操作）。
    /// </summary>
    public async Task<VariableDto> CreateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.CreateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量（事务性操作）。
    /// </summary>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.UpdateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步批量更新变量（事务性操作）。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        return await _variableAppService.UpdateVariablesAsync(variableDtos);
    }

    /// <summary>
    /// 异步删除一个变量（事务性操作）。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        return await _variableAppService.DeleteVariableAsync(id);
    }

    /// <summary>
    /// 异步批量删除变量（事务性操作）。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        return await _variableAppService.DeleteVariablesAsync(ids);
    }

    /// <summary>
    /// 异步批量导入变量。
    /// </summary>
    public async Task<bool> BatchImportVariablesAsync(List<VariableDto> variables)
    {
        return await _variableAppService.BatchImportVariablesAsync(variables);
    }

    /// <summary>
    /// 检测一组变量是否已存在。
    /// </summary>
    public async Task<List<VariableDto>> FindExistingVariablesAsync(IEnumerable<VariableDto> variablesToCheck)
    {
        return await _variableAppService.FindExistingVariablesAsync(variablesToCheck);
    }

    /// <summary>
    /// 检测单个变量是否已存在。
    /// </summary>
    public async Task<VariableDto?> FindExistingVariableAsync(VariableDto variableToCheck)
    {
        return await _variableAppService.FindExistingVariableAsync(variableToCheck);
    }

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    public void AddVariableToMemory(VariableDto variableDto)
    {
        Variables.TryAdd(variableDto.Id, variableDto);
    }

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    public void UpdateVariableInMemory(VariableDto variableDto)
    {
        Variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
    }

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    public void RemoveVariableFromMemory(int variableId)
    {
        Variables.TryRemove(variableId, out _);
    }

    /// <summary>
    /// 批量在内存中添加变量
    /// </summary>
    public void AddVariablesToMemory(List<VariableDto> variables)
    {
        foreach (var variable in variables)
        {
            Variables.TryAdd(variable.Id, variable);
        }
    }

    /// <summary>
    /// 批量在内存中更新变量
    /// </summary>
    public void UpdateVariablesInMemory(List<VariableDto> variables)
    {
        foreach (var variable in variables)
        {
            Variables.AddOrUpdate(variable.Id, variable, (key, oldValue) => variable);
        }
    }

    /// <summary>
    /// 批量在内存中删除变量
    /// </summary>
    public void RemoveVariablesFromMemory(List<int> variableIds)
    {
        foreach (var variableId in variableIds)
        {
            Variables.TryRemove(variableId, out _);
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

            // 加载所有设备
            var devices = await _repositoryManager.Devices.GetAllAsync();
            var deviceDtos = _mapper.Map<List<DeviceDto>>(devices);
            
            // 加载所有变量表
            var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
            var variableTableDtos = _mapper.Map<List<VariableTableDto>>(variableTables);
            
            // 加载所有变量
            var variables = await _repositoryManager.Variables.GetAllAsync();
            var variableDtos = _mapper.Map<List<VariableDto>>(variables);

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
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"加载所有数据到内存时发生错误,错误信息:{ex.Message}", ex);
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