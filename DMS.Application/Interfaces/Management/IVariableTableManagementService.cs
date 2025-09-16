using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;

namespace DMS.Application.Interfaces.Management;

public interface IVariableTableManagementService
{
    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

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

    /// <summary>
    /// 在内存中添加变量表
    /// </summary>
    void AddVariableTableToMemory(VariableTableDto variableTableDto);

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    void UpdateVariableTableInMemory(VariableTableDto variableTableDto);

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    void RemoveVariableTableFromMemory(int variableTableId);
}