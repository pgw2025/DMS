using DMS.Application.DTOs;

namespace DMS.Application.Interfaces.Database
{
    public interface IVariableTableAppService
    {
        Task<VariableTableDto> GetVariableTableByIdAsync(int id);
        Task<List<VariableTableDto>> GetAllVariableTablesAsync();
        Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto createDto);
        Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto);
        Task<bool> DeleteVariableTableAsync(int id);
    }
}
