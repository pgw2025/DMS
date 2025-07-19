using DMS.Config;
using DMS.Infrastructure.Interfaces;
using SqlSugar;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Data;

public class SqlSugarDbContext : ITransaction
{
    private readonly SqlSugarClient _db;

    public SqlSugarDbContext(ConnectionSettings settings)
    {
        var connectionString = settings.ToConnectionString();
        var dbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), settings.DbType);

        _db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }


    public async Task BeginTranAsync()
    {
        await _db.BeginTranAsync();
    }

    public async Task CommitTranAsync()
    {
        await _db.CommitTranAsync();
    }

  

    public async Task RollbackTranAsync()
    {
        await _db.RollbackTranAsync();
    }

    public SqlSugarClient GetInstance()
    {
        return _db;
    }
}