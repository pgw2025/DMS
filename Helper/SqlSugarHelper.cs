using PMSWPF.Data;

namespace PMSWPF.Helper;

public class SqlSugarHelper
{
    private DbContext _db;

    public SqlSugarHelper()
    {
        _db = new DbContext();
    }

    public void InitTables()
    {
    }
}