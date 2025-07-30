
using DMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces
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
