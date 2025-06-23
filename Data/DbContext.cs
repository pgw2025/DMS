using SqlSugar;

namespace PMSWPF.Data;

public class DbContext
{
    private static SqlSugarClient _db;

    public static SqlSugarClient GetInstance()
    {
        if (_db == null)
        {
            var connectionString = "server=127.0.0.1;port=3306;user=root;password=Pgw15221236646; database=pmswpf;";
            _db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = DbType.MySql, // 根据实际数据库类型修改，如DbType.MySql等
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        return _db;
    }
}