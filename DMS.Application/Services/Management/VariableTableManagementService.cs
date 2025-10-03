using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.Events;
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
    private readonly IEventService _eventService;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

    public VariableTableManagementService(IVariableTableAppService variableTableAppService, 
                                         IAppDataStorageService appDataStorageService,
                                         IEventService eventService)
    {
        _variableTableAppService = variableTableAppService;
        _appDataStorageService = appDataStorageService;
        _eventService = eventService;
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
        var result = await _variableTableAppService.CreateVariableTableAsync(dto);
        
        // 创建成功后，将变量表添加到内存中
        if (result?.VariableTable != null)
        {
            // 添加null检查
            if (result.VariableTable == null)
                return result;

            DeviceDto deviceDto = null;
            if (_appDataStorageService.Devices != null && 
                _appDataStorageService.Devices.TryGetValue(result.VariableTable.DeviceId, out var device))
            {
                deviceDto = device;
                // 确保VariableTables不为null
                if (device.VariableTables == null)
                    device.VariableTables = new List<VariableTableDto>();
                    
                device.VariableTables.Add(result.VariableTable);
                
                // 确保Device属性不为null
                if (result.VariableTable != null)
                    result.VariableTable.Device = device;
            }

            // 确保_variableTables和result.VariableTable不为null
            if (_appDataStorageService.VariableTables.TryAdd(result.VariableTable.Id, result.VariableTable))
            {
                _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(
                                           DataChangeType.Added,
                                           result.VariableTable));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的变量表。
    /// </summary>
    public async Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto)
    {
        var result = await _variableTableAppService.UpdateVariableTableAsync(variableTableDto);
        
        // 更新成功后，更新内存中的变量表
        if (result > 0 && variableTableDto != null)
        {
            DeviceDto deviceDto = null;
            if (_appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var device))
            {
                deviceDto = device;
            }

            _appDataStorageService.VariableTables.AddOrUpdate(variableTableDto.Id, variableTableDto, (key, oldValue) => variableTableDto);
            _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(
                                             DataChangeType.Updated,
                                             variableTableDto));
        }
        
        return result;
    }

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTableAsync(int id)
    {
        var variableTable = await _variableTableAppService.GetVariableTableByIdAsync(id); // 获取变量表信息用于内存删除
        var result = await _variableTableAppService.DeleteVariableTableAsync(id);
        
        // 删除成功后，从内存中移除变量表
        if (result && variableTable != null)
        {
            if (_appDataStorageService.VariableTables.TryRemove(id, out var variableTableDto))
            {
                DeviceDto deviceDto = null;
                if (variableTableDto != null && _appDataStorageService.Devices.TryGetValue(variableTableDto.DeviceId, out var device))
                {
                    deviceDto = device;
                    if (device.VariableTables != null)
                        device.VariableTables.Remove(variableTableDto);
                }

                _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(
                                                 DataChangeType.Deleted,
                                                 variableTableDto));
            }
        }
        
        return result;
    }

    

  
}