using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using System.Collections.ObjectModel;
using DMS.Application.Services.Management;

namespace DMS.WPF.Services;

/// <summary>
/// 变量数据服务类，负责管理变量表和变量相关的数据和操作。
/// </summary>
public class VariableDataService : IVariableDataService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IAppDataCenterService _appDataCenterService;



    /// <summary>
    /// VariableDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataCenterService">数据服务中心实例。</param>
    public VariableDataService(IMapper mapper, IDataStorageService dataStorageService, IAppDataCenterService appDataCenterService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataCenterService = appDataCenterService;
        
        // 订阅批量导入变量事件
        if (_appDataCenterService.VariableManagementService is VariableManagementService variableManagementService)
        {
            // 如果需要直接订阅事件，这将需要EventService实例
        }
    }

    /// <summary>
    /// 加载所有变量
    /// </summary>
    public void LoadAllVariables()
    {
        foreach (var variableTable in _dataStorageService.VariableTables)
        {
            foreach (var variable in variableTable.Value.Variables)
            {
                _dataStorageService.Variables.Add(variable.Id, variable);
            }
        }
    }

    /// <summary>
    /// 添加变量表。
    /// </summary>
    public async Task<bool> AddVariableTableToView(VariableTable tableDto)
    {
        // 添加null检查
        if (tableDto == null || tableDto.DeviceId==0)
            return false;

        if (_dataStorageService.Devices.TryGetValue(tableDto.DeviceId, out var device))
        {
            var variableTableItem = _mapper.Map<VariableTableItem>(tableDto);
            device.VariableTables.Add(variableTableItem);
            _dataStorageService.VariableTables.TryAdd(variableTableItem.Id,variableTableItem);
        }

    
        return true;
    }

    /// <summary>
    /// 更新变量表。
    /// </summary>
    public async Task<bool> UpdateVariableTable(VariableTableItem variableTableItem)
    {
        if (variableTableItem is null)
        {
            return false;
        }

        var variableTable = _mapper.Map<VariableTable>(variableTableItem);
        if (await _appDataCenterService.VariableTableManagementService.UpdateVariableTableAsync(variableTable) > 0)
        {
            // 更新数据库后会自动更新内存，无需额外操作
            return true;
        }

        return false;
    }

    /// <summary>
    /// 删除变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTable(VariableTableItem variableTable, bool isDeleteDb = false)
    {
        if (variableTable == null)
        {
            return false;
        }

        if (isDeleteDb)
        {
            if (!await _appDataCenterService.VariableTableManagementService.DeleteVariableTableAsync(variableTable.Id))
            {
                return false;
            }
        }

        // 删除与当前变量表关联的所有变量
        foreach (var variable in variableTable.Variables)
        {
            _dataStorageService.Variables.Remove(variable.Id);
        }

        // 删除变量表
        _dataStorageService.VariableTables.Remove(variableTable.Id);
        variableTable.Device.VariableTables.Remove(variableTable);
        return true;
    }

    /// <summary>
    /// 添加变量。
    /// </summary>
    public void AddVariable(VariableItem variableItem)
    {
        if (variableItem == null)
        {
            return;
        }

        _dataStorageService.Variables.Add(variableItem.Id, variableItem);
    }

    /// <summary>
    /// 删除变量。
    /// </summary>
    public void DeleteVariable(int id)
    {
        if (!_dataStorageService.Variables.TryGetValue(id, out var variableItem))
        {
            return;
        }

        if (_dataStorageService.VariableTables.TryGetValue(variableItem.VariableTableId, out var variableTable))
        {
            variableTable.Variables.Remove(variableItem);
        }

        _dataStorageService.Variables.Remove(variableItem.Id);
    }
}