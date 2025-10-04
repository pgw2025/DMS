using DMS.Application.Configurations;
using SqlSugar;

namespace DMS.Infrastructure.Data;

public class SqlSugarDbContext 
{
    private readonly AppSettings _settings;

    public SqlSugarDbContext(AppSettings settings)
    {
        _settings = settings;
    }


    public SqlSugarClient GetInstance()
    {
        var connectionString = _settings.ToConnectionString();
        var dbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), _settings.Db.DbType);

        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }
}