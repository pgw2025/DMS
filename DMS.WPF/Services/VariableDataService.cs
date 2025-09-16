using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

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
    public VariableDataService(IMapper mapper,IDataStorageService dataStorageService, IAppDataCenterService appDataCenterService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataCenterService = appDataCenterService;
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
                _dataStorageService.Variables.Add(variable.Id,variable);
            }
        }
    }

    /// <summary>
    /// 添加变量表。
    /// </summary>
    public async Task<bool> AddVariableTable(VariableTableDto variableTableDto,
                                             MenuBeanDto menuDto = null, bool isAddDb = false)
    {
        // 添加null检查
        if (variableTableDto == null)
            return false;

        // 添加_appDataCenterService和_variableTableManagementService的null检查
        if (_appDataCenterService == null || _appDataCenterService.VariableTableManagementService == null)
            return false;

        if (isAddDb && menuDto != null)
        {
            // 添加null检查
            if (_appDataCenterService.VariableTableManagementService == null)
                return false;

            CreateVariableTableWithMenuDto createDto = new CreateVariableTableWithMenuDto();
            createDto.VariableTable = variableTableDto;
            createDto.DeviceId = variableTableDto.DeviceId;
            createDto.Menu = menuDto;
            
            // 添加null检查
            if (_appDataCenterService.VariableTableManagementService == null)
                return false;
                
            var resDto = await _appDataCenterService.VariableTableManagementService.CreateVariableTableAsync(createDto);
            
            // 添加null检查
            if (resDto != null && resDto.VariableTable != null && variableTableDto != null)
                _mapper.Map(resDto.VariableTable, variableTableDto);
        }

        // 添加null检查
        if (_appDataCenterService.VariableTableManagementService != null && variableTableDto != null)
            _appDataCenterService.VariableTableManagementService.AddVariableTableToMemory(variableTableDto);

        return true;
    }

    /// <summary>
    /// 更新变量表。
    /// </summary>
    public async Task<bool> UpdateVariableTable(VariableTableItemViewModel variableTable)
    {
        if (variableTable == null)
        {
            return false;
        }

        var variableTableDto = _mapper.Map<VariableTableDto>(variableTable);
        if (await _appDataCenterService.VariableTableManagementService.UpdateVariableTableAsync(variableTableDto) > 0)
        {
            _appDataCenterService.VariableTableManagementService.UpdateVariableTableInMemory(variableTableDto);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 删除变量表。
    /// </summary>
    public async Task<bool> DeleteVariableTable(VariableTableItemViewModel variableTable, bool isDeleteDb = false)
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

        _appDataCenterService.VariableTableManagementService.RemoveVariableTableFromMemory(variableTable.Id);

        // 删除变量表
        _dataStorageService.VariableTables.Remove(variableTable.Id);
        variableTable.Device.VariableTables.Remove(variableTable);
        return true;
    }

    /// <summary>
    /// 添加变量。
    /// </summary>
    public void AddVariable(VariableItemViewModel variableItem)
    {
        if (variableItem == null)
        {
            return;
        }

        _dataStorageService.Variables.Add(variableItem.Id,variableItem);
    }

    /// <summary>
    /// 删除变量。
    /// </summary>
    public void DeleteVariable(int id)
    {
        if (!_dataStorageService.Variables.TryGetValue(id,out var variableItem))
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