using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.Data.Repositories;

public class VarTableRepository
{
    /// <summary>
    /// 添加变量表
    /// </summary>
    /// <param name="varTable"></param>
    /// <returns>变量表的ID</returns>
    public async Task<int> Add(VariableTable varTable)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Insertable<DbVariableTable>(varTable.CopyTo<DbVariableTable>()).ExecuteReturnIdentityAsync();
        }
    }

    public async Task<int> Edit(VariableTable variableTable)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Updateable<DbVariableTable>(variableTable.CopyTo<DbVariableTable>()).ExecuteCommandAsync();
        }
    }
}   