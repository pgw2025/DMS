using System.Collections.ObjectModel;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Services;

public class VariableTableDataService : IVariableTableDataService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IMenuDataService _menuDataService;



    public VariableTableDataService(IMapper mapper, IDataStorageService dataStorageService, IAppDataCenterService appDataCenterService,
                                    IMenuDataService menuDataService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataCenterService = appDataCenterService;
        _menuDataService = menuDataService;
    }

    
    public void LoadAllVariableTables()
    {
        foreach (var device in _dataStorageService.Devices)
        {
            foreach (var variableTable in device.Value.VariableTables)
            {
                _dataStorageService.VariableTables.Add(variableTable.Id,variableTable);
            }
        }
    }

    public async Task<int> AddVariableTable(VariableTable variableTable,
                                             MenuBean menuDto = null, bool isAddDb = false)
    {
        if (variableTable == null)
            return 0;

        if (isAddDb && menuDto != null)
        {
            CreateVariableTableWithMenuDto createDto = new CreateVariableTableWithMenuDto();
            createDto.VariableTable = variableTable;
            createDto.DeviceId = variableTable.DeviceId;
            createDto.Menu = menuDto;
            var resDto = await _appDataCenterService.VariableTableManagementService.CreateVariableTableAsync(createDto);
            
            await _menuDataService.AddMenuItem(_mapper.Map<MenuItem>(resDto.Menu));
            return resDto.VariableTable.Id;
        }



        return 0;
    }


    public async Task<bool> UpdateVariableTable(VariableTableItem variableTable)
    {
        if (variableTable == null)
        {
            return false;
        }

        var variableTable_mapped = _mapper.Map<VariableTable>(variableTable);
        if (await _appDataCenterService.VariableTableManagementService.UpdateVariableTableAsync(variableTable_mapped) > 0)
        {
            // 更新数据库后会自动更新内存，无需额外操作

            var menu = _dataStorageService.Menus.FirstOrDefault(m =>
                                                                    m.MenuType == MenuType.VariableTableMenu &&
                                                                    m.TargetId == variableTable.Id);
            if (menu != null)
            {
                menu.Header = variableTable.Name;
            }

            return true;
        }

        return false;
    }

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

        // 删除变量表界面相关的菜单

        var variableTableMenu
            =_dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu && m.TargetId == variableTable.Id);
       await _menuDataService.DeleteMenuItem(variableTableMenu);
        // 删除变量表
        _dataStorageService.VariableTables.Remove(variableTable.Id);
        variableTable.Device.VariableTables.Remove(variableTable);
        return true;
    }
}