using DMS.Config;
using SqlSugar;

namespace DMS.Data;

public class DbContext
{
    public static SqlSugarClient GetInstance()
    {
        var settings = ConnectionSettings.Load();
        var connectionString = settings.ToConnectionString();
        var dbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), settings.DbType);

        var _db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType, // 根据实际数据库类型修改，如DbType.MySql等
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });


        return _db;
    }
}