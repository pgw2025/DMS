using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

public interface IVariableTableDataService
{

    void LoadAllVariableTables();

    Task<int> AddVariableTable(VariableTableDto variableTableDto,
                                MenuBeanDto menuDto = null, bool isAddDb = false);

    Task<bool> UpdateVariableTable(VariableTableItemViewModel variableTable);
    Task<bool> DeleteVariableTable(VariableTableItemViewModel variableTable, bool isDeleteDb = false);
}