using System.Collections.Concurrent;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Events;
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
    private readonly IMapper _mapper;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataProcessingService _dataProcessingService;


    public VariableManagementService(IVariableAppService variableAppService,
                                     IEventService eventService,
                                     IMapper mapper,
                                     IAppDataStorageService appDataStorageService,
                                     IDataProcessingService dataProcessingService)
    {
        _variableAppService = variableAppService;
        _eventService = eventService;
        _mapper = mapper;
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
        var result = await _variableAppService.CreateVariableAsync(variableDto);
        
        // 创建成功后，将变量添加到内存中
        if (result != null)
        {
            VariableTableDto variableTableDto = null;
            if (_appDataStorageService.VariableTables.TryGetValue(result.VariableTableId, out var variableTable))
            {
                variableTableDto = variableTable;
                result.VariableTable = variableTableDto;
                variableTable.Variables.Add(result);
            }

            if (_appDataStorageService.Variables.TryAdd(result.Id, result))
            {
                _eventService.RaiseVariableChanged(
                    this, new VariableChangedEventArgs(DataChangeType.Added, result, variableTableDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        var result = await _variableAppService.UpdateVariableAsync(variableDto);
        
        // 更新成功后，更新内存中的变量
        if (result > 0 && variableDto != null)
        {
            VariableTableDto variableTableDto = null;
            if (_appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
            {
                variableTableDto = variableTable;
            }

            _appDataStorageService.Variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
            _eventService.RaiseVariableChanged(
                this, new VariableChangedEventArgs(DataChangeType.Updated, variableDto, variableTableDto));
        }
        
        return result;
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        var result = await _variableAppService.UpdateVariablesAsync(variableDtos);
        
        // 批量更新成功后，更新内存中的变量
        if (result > 0 && variableDtos != null)
        {
            foreach (var variableDto in variableDtos)
            {
                VariableTableDto variableTableDto = null;
                if (_appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
                {
                    variableTableDto = variableTable;
                }

                if (_appDataStorageService.Variables.TryGetValue(variableDto.Id,out var mVariableDto))
                {
                    _mapper.Map(variableDto, mVariableDto);
                }

                // _appDataStorageService.Variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
                _eventService.RaiseVariableChanged(
                    this, new VariableChangedEventArgs(DataChangeType.Updated, variableDto, variableTableDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        var variable = await _variableAppService.GetVariableByIdAsync(id); // 获取变量信息用于内存删除
        var result = await _variableAppService.DeleteVariableAsync(id);
        
        // 删除成功后，从内存中移除变量
        if (result && variable != null)
        {
            if (_appDataStorageService.Variables.TryRemove(id, out var variableDto))
            {
                VariableTableDto variableTableDto = null;
                if (variableDto != null && _appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
                {
                    variableTableDto = variableTable;
                    variableTable.Variables.Remove(variableDto);
                }

                _eventService.RaiseVariableChanged(
                    this, new VariableChangedEventArgs(DataChangeType.Deleted, variableDto, variableTableDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步批量导入变量。
    /// </summary>
    public async Task<List<VariableDto>> BatchImportVariablesAsync(List<VariableDto> variables)
    {
        var result = await _variableAppService.BatchImportVariablesAsync(variables);
        foreach (var variableDto in result)
        {
            if (_appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId ,out var variableTable))
            {
                variableDto.VariableTable = variableTable;
            }
            
        }
        
        
        // 批量导入成功后，触发批量导入事件
        if (result != null && result.Any())
        {
            _eventService.RaiseBatchImportVariables(this, new BatchImportVariablesEventArgs(result));
        }
        
        return result;
    }

    public async Task<List<VariableDto>> FindExistingVariablesAsync(IEnumerable<VariableDto> variablesToCheck)
    {
        return await _variableAppService.FindExistingVariablesAsync(variablesToCheck);
    }

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        var result = await _variableAppService.DeleteVariablesAsync(ids);
        
        // 批量删除成功后，从内存中移除变量
        if (result && ids != null)
        {
            foreach (var id in ids)
            {
                if (_appDataStorageService.Variables.TryRemove(id, out var variableDto))
                {
                    VariableTableDto variableTableDto = null;
                    if (variableDto != null && _appDataStorageService.VariableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
                    {
                        variableTableDto = variableTable;
                        variableTable.Variables.Remove(variableDto);
                    }

                    _eventService.RaiseVariableChanged(
                        this, new VariableChangedEventArgs(DataChangeType.Deleted, variableDto, variableTableDto));
                }
            }
        }
        
        return result;
    }

    
}