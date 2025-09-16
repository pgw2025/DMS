using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// 变量管理服务，负责变量相关的业务逻辑。
/// </summary>
public class VariableManagementService : IVariableManagementService
{
    private readonly IVariableAppService _variableAppService;
    private readonly IEventService _eventService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataProcessingService _dataProcessingService;


    public VariableManagementService(IVariableAppService variableAppService,
                                     IEventService eventService,
                                     IAppDataStorageService appDataStorageService,
                                     IDataProcessingService dataProcessingService)
    {
        _variableAppService = variableAppService;
        _eventService = eventService;
        _appDataStorageService = appDataStorageService;
        _dataProcessingService = dataProcessingService;
    }

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
    public void AddVariableToMemory(VariableDto variableDto, ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        VariableTableDto variableTableDto = null;
        if (variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
            variableDto.VariableTable = variableTableDto;
            variableTable.Variables.Add(variableDto);
        }

        if (_appDataStorageService.Variables.TryAdd(variableDto.Id, variableDto))
        {
            _eventService.RaiseVariableChanged(
                this, new VariableChangedEventArgs(DataChangeType.Added, variableDto, variableTableDto));
        }
    }

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    public void UpdateVariableInMemory(VariableDto variableDto,
                                       ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        VariableTableDto variableTableDto = null;
        if (variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
        }

        _appDataStorageService.Variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
        _eventService.RaiseVariableChanged(
            this, new VariableChangedEventArgs(DataChangeType.Updated, variableDto, variableTableDto));
    }

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    public void RemoveVariableFromMemory(int variableId, ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        if (_appDataStorageService.Variables.TryRemove(variableId, out var variableDto))
        {
            VariableTableDto variableTableDto = null;
            if (variableDto != null && variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
            {
                variableTableDto = variableTable;
                variableTable.Variables.Remove(variableDto);
            }

            _eventService.RaiseVariableChanged(
                this, new VariableChangedEventArgs(DataChangeType.Deleted, variableDto, variableTableDto));
        }
    }
}