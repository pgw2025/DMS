
using DMS.Application.DTOs;
using DMS.Core.Models;

namespace DMS.Application.Interfaces.Database
{
    public interface IVariableTableAppService
    {
        Task<VariableTable> GetVariableTableByIdAsync(int id);
        Task<List<VariableTable>> GetAllVariableTablesAsync();
        Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto createDto);
        Task<int> UpdateVariableTableAsync(VariableTable variableTableDto);
        Task<bool> DeleteVariableTableAsync(int id);
    }
}
