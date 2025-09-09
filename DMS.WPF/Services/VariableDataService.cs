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
            foreach (var variable in variableTable.Variables)
            {
                _dataStorageService.Variables.Add(variable);
            }
        }
    }

    /// <summary>
    /// 添加变量表。
    /// </summary>
    public async Task<bool> AddVariableTable(VariableTableDto variableTableDto,
                                             MenuBeanDto menuDto = null, bool isAddDb = false)
    {
        if (variableTableDto == null)
            return false;

        if (isAddDb && menuDto != null)
        {
            CreateVariableTableWithMenuDto createDto = new CreateVariableTableWithMenuDto();
            createDto.VariableTable = variableTableDto;
            createDto.DeviceId = variableTableDto.DeviceId;
            createDto.Menu = menuDto;
            var resDto = await _appDataCenterService.VariableTableManagementService.CreateVariableTableAsync(createDto);
            _mapper.Map(resDto.VariableTable, variableTableDto);
        }

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
            _dataStorageService.Variables.Remove(variable);
        }

        _appDataCenterService.VariableTableManagementService.RemoveVariableTableFromMemory(variableTable.Id);

        // 删除变量表
        _dataStorageService.VariableTables.Remove(variableTable);
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

        _dataStorageService.Variables.Add(variableItem);
    }

    /// <summary>
    /// 删除变量。
    /// </summary>
    public void DeleteVariable(int id)
    {
        var variableItem = _dataStorageService.Variables.FirstOrDefault(v => v.Id == id);
        if (variableItem == null)
        {
            return;
        }

        var variableTable = _dataStorageService.VariableTables.FirstOrDefault(vt => vt.Id == variableItem.VariableTableId);

        variableTable.Variables.Remove(variableItem);

        _dataStorageService.Variables.Remove(variableItem);
    }
}