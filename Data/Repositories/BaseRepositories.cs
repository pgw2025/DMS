using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class BaseRepositories
{
    protected readonly SqlSugarClient _db;

    public BaseRepositories()
    {
        _db = DbContext.GetInstance();
    }
}