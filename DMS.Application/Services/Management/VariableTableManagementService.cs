using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

/// <summary>
/// 变量表管理服务，负责变量表相关的业务逻辑。
/// </summary>
public class VariableTableManagementService : IVariableTableManagementService
{
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly IAppStorageService _appStorageService;
    private readonly IEventService _eventService;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

    public VariableTableManagementService(IVariableTableAppService variableTableAppService,
                                         IAppStorageService appStorageService,
                                         IEventService eventService)
    {
        _variableTableAppService = variableTableAppService;
        _appStorageService = appStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步根据ID获取变量表DTO。
    /// </summary>
    public async Task<VariableTable> GetVariableTableByIdAsync(int id)
    {
        return await _variableTableAppService.GetVariableTableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量表DTO列表。
    /// </summary>
    public async Task<List<VariableTable>> GetAllVariableTablesAsync()
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
            if (_appStorageService.Devices != null &&
                _appStorageService.Devices.TryGetValue(result.VariableTable.DeviceId, out var device))
            {
                // 确保VariableTables不为null
                if (device.VariableTables == null)
                    device.VariableTables = new List<VariableTable>();

                device.VariableTables.Add(result.VariableTable);

                // 确保Device属性不为null
                if (result.VariableTable != null)
                    result.VariableTable.Device = device;
            }

            // 确保_variableTables和result.VariableTable不为null
            if (_appStorageService.VariableTables.TryAdd(result.VariableTable.Id, result.VariableTable))
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
    public async Task<int> UpdateVariableTableAsync(VariableTable variableTable)
    {
        var result = await _variableTableAppService.UpdateVariableTableAsync(variableTable);

        // 更新成功后，更新内存中的变量表
        if (result > 0 && variableTable != null)
        {
            _appStorageService.VariableTables.AddOrUpdate(variableTable.Id, variableTable, (key, oldValue) => variableTable);
            _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(
                                             DataChangeType.Updated,
                                             variableTable));
        }

        return result;
    }

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTableAsync(int id)
    {
        var result = await _variableTableAppService.DeleteVariableTableAsync(id);

        // 删除成功后，从内存中移除变量表
        if (result )
        {
            if (_appStorageService.VariableTables.TryRemove(id, out var variableTable))
            {
                if (variableTable != null && _appStorageService.Devices.TryGetValue(variableTable.DeviceId, out var device))
                {
                    if (device.VariableTables != null)
                        device.VariableTables.Remove(variableTable);
                }

                _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(
                                                 DataChangeType.Deleted,
                                                 variableTable));
            }
        }

        return result;
    }


    /// <summary>
    /// 异步加载所有变量表数据到内存中。
    /// </summary>
    public async Task LoadAllVariableTablesAsync()
    {
        _appStorageService.VariableTables.Clear();
        var variableTables = await _variableTableAppService.GetAllVariableTablesAsync();
        // 建立变量表与变量的关联
        foreach (var variableTable in variableTables)
        {
            if (_appStorageService.Devices.TryGetValue(variableTable.DeviceId, out var device))
            {
                variableTable.Device = device;
                if (device.VariableTables == null)
                    device.VariableTables = new List<VariableTable>();
                device.VariableTables.Add(variableTable);
            }

            // 将变量表添加到安全字典
            _appStorageService.VariableTables.TryAdd(variableTable.Id, variableTable);
        }
    }

}