using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DMS.Application.Services;

/// <summary>
/// 变量表管理服务，负责变量表相关的业务逻辑。
/// </summary>
public class VariableTableManagementService
{
    private readonly IVariableTableAppService _variableTableAppService;
    private readonly ConcurrentDictionary<int, VariableTableDto> _variableTables;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> VariableTableChanged;

    public VariableTableManagementService(IVariableTableAppService variableTableAppService, 
                                         ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        _variableTableAppService = variableTableAppService;
        _variableTables = variableTables;
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
    public void AddVariableTableToMemory(VariableTableDto variableTableDto, ConcurrentDictionary<int, DeviceDto> devices)
    {
        DeviceDto deviceDto = null;
        if (devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
            device.VariableTables.Add(variableTableDto);
            variableTableDto.Device = device;
        }

        if (_variableTables.TryAdd(variableTableDto.Id, variableTableDto))
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
    public void UpdateVariableTableInMemory(VariableTableDto variableTableDto, ConcurrentDictionary<int, DeviceDto> devices)
    {
        DeviceDto deviceDto = null;
        if (devices.TryGetValue(variableTableDto.DeviceId, out var device))
        {
            deviceDto = device;
        }

        _variableTables.AddOrUpdate(variableTableDto.Id, variableTableDto, (key, oldValue) => variableTableDto);
        OnVariableTableChanged(new VariableTableChangedEventArgs(
                                   DataChangeType.Updated,
                                   variableTableDto,
                                   deviceDto));
    }

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    public void RemoveVariableTableFromMemory(int variableTableId, ConcurrentDictionary<int, DeviceDto> devices)
    {
        if (_variableTables.TryRemove(variableTableId, out var variableTableDto))
        {
            DeviceDto deviceDto = null;
            if (variableTableDto != null && devices.TryGetValue(variableTableDto.DeviceId, out var device))
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

    /// <summary>
    /// 触发变量表变更事件
    /// </summary>
    protected virtual void OnVariableTableChanged(VariableTableChangedEventArgs e)
    {
        VariableTableChanged?.Invoke(this, e);
    }
}