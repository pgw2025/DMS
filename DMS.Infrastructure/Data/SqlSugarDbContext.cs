using DMS.Config;
using DMS.Infrastructure.Interfaces;
using SqlSugar;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Data;

public class SqlSugarDbContext 
{
    private readonly SqlSugarClient _db;

    public SqlSugarDbContext(AppSettings settings)
    {
        var connectionString = settings.ToConnectionString();
        var dbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), settings.Database.DbType);

        _db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }


    public SqlSugarClient GetInstance()
    {
        return _db;
    }
}