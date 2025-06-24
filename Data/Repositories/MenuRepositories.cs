using System.Windows.Controls;
using PMSWPF.Data.Entities;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class MenuRepositories
{
    private readonly SqlSugarClient _db;

    public MenuRepositories()
    {
        _db=DbContext.GetInstance();
    }

    public async Task<List<DbMenu>> GetMenu()
    {
     return await  _db.Queryable<DbMenu>().ToListAsync();
    }
    
    public async Task<int> AddMenu(DbMenu dbMenu)
    {
        return await _db.Insertable<DbMenu>(dbMenu).ExecuteCommandAsync();
    }
}