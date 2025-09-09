using System.Collections.ObjectModel;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

public class VariableTableDataService : IVariableTableDataService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IMenuDataService _menuDataService;
    // Removed circular dependency by not injecting IDeviceDataService
    // private readonly IDeviceDataService _deviceDataService;



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
        foreach (var variableTable in _dataStorageService.VariableTables)
        {
            _dataStorageService.VariableTables.Add(variableTable);
        }
    }

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
            _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(resDto.Menu));
        }

        _appDataCenterService.VariableTableManagementService.AddVariableTableToMemory(variableTableDto);

        // var device = _deviceDataService.Devices.FirstOrDefault(d => d.Id == variableTableDto.DeviceId);
        // if (device != null)
        // {
        //     var variableTableItemViewModel = _mapper.Map<VariableTableItemViewModel>(variableTableDto);
        //     variableTableItemViewModel.Device = device;
        //     device.VariableTables.Add(variableTableItemViewModel);
        //     VariableTables.Add(variableTableItemViewModel);
        // }

        return true;
    }


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

        var variableTableMenu
            =_dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu && m.TargetId == variableTable.Id);
        _menuDataService.DeleteMenuItem(variableTableMenu);
        // 删除变量表
        _dataStorageService.VariableTables.Remove(variableTable);
        variableTable.Device.VariableTables.Remove(variableTable);
        return true;
    }
}