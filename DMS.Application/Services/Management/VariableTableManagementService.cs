using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// 变量表管理服务，负责变量表相关的业务逻辑。
/// </summary>
public class VariableTableManagementService : IVariableTableManagementService
{
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly ConcurrentDictionary<int, VariableTableDto> _variableTables;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

    public VariableTableManagementService(IVariableTableAppService variableTableAppService,IAppDataStorageService appDataStorageService 
                                         )
    {
        _variableTableAppService = variableTableAppService;
        _appDataStorageService = appDataStorageService;
    }

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
        // 添加null检查
        if (variableTableDto == null)
            return;

        DeviceDto deviceDto = null;
        if (_appDataStorageService.Devices != null && 
            _appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
            // 确保VariableTables不为null
            if (device.VariableTables == null)
                device.VariableTables = new List<VariableTableDto>();
                
            device.VariableTables.Add(variableTableDto);
            
            // 确保Device属性不为null
            if (variableTableDto != null)
                variableTableDto.Device = device;
        }

        // 确保_variableTables和variableTableDto不为null
        if (_variableTables != null && variableTableDto != null)
        {
            if (_variableTables.TryAdd(variableTableDto.Id, variableTableDto))
            {
                OnVariableTableChanged?.Invoke(this, new VariableTableChangedEventArgs(
                                           DataChangeType.Added,
                                           variableTableDto,
                                           deviceDto));
            }
        }
    }

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    public void UpdateVariableTableInMemory(VariableTableDto variableTableDto)
    {
        DeviceDto deviceDto = null;
        if (_appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
        }

        _variableTables.AddOrUpdate(variableTableDto.Id, variableTableDto, (key, oldValue) => variableTableDto);
        OnVariableTableChanged?.Invoke(this,new VariableTableChangedEventArgs(
                                         DataChangeType.Updated,
                                         variableTableDto,
                                         deviceDto));
    }

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    public void RemoveVariableTableFromMemory(int variableTableId)
    {
        if (_variableTables.TryRemove(variableTableId, out var variableTableDto))
        {
            DeviceDto deviceDto = null;
            if (variableTableDto != null && _appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var device))
            {
                deviceDto = device;
                device.VariableTables.Remove(variableTableDto);
            }

            OnVariableTableChanged?.Invoke(this,new VariableTableChangedEventArgs(
                                             DataChangeType.Deleted,
                                             variableTableDto,
                                             deviceDto));
        }
    }

  
}