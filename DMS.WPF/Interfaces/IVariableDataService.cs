using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 变量数据服务接口。
/// </summary>
public interface IVariableDataService
{

    /// <summary>
    /// 加载所有变量
    /// </summary>
     void LoadAllVariables();

    /// <summary>
    /// 添加变量表。
    /// </summary>
    Task<bool> AddVariableTableToView(VariableTableDto tableDto);

    /// <summary>
    /// 更新变量表。
    /// </summary>
    Task<bool> UpdateVariableTable(VariableTableItemViewModel variableTable);

    /// <summary>
    /// 删除变量表。
    /// </summary>
    Task<bool> DeleteVariableTable(VariableTableItemViewModel variableTable, bool isDeleteDb = false);

    /// <summary>
    /// 添加变量。
    /// </summary>
    void AddVariable(VariableItemViewModel variableItem);

    /// <summary>
    /// 删除变量。
    /// </summary>
    void DeleteVariable(int id);
}