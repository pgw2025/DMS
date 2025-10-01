using DMS.Application.DTOs;
using DMS.Application.Events;

namespace DMS.Application.Interfaces.Management;

public interface IVariableTableManagementService
{
    /// <summary>
    /// 异步根据ID获取变量表DTO。
    /// </summary>
    Task<VariableTableDto> GetVariableTableByIdAsync(int id);

    /// <summary>
    /// 异步获取所有变量表DTO列表。
    /// </summary>
    Task<List<VariableTableDto>> GetAllVariableTablesAsync();

    /// <summary>
    /// 异步创建一个新变量表及其关联菜单（事务性操作）。
    /// </summary>
    Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto dto);

    /// <summary>
    /// 异步更新一个已存在的变量表。
    /// </summary>
    Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto);

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    Task<bool> DeleteVariableTableAsync(int id);
}