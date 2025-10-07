using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Interfaces;

public interface IVariableTableDataService
{

    void LoadAllVariableTables();

    Task<int> AddVariableTable(VariableTable variableTable,
                                MenuBeanDto menuDto = null, bool isAddDb = false);

    Task<bool> UpdateVariableTable(VariableTableItem variableTable);
    Task<bool> DeleteVariableTable(VariableTableItem variableTable, bool isDeleteDb = false);
}