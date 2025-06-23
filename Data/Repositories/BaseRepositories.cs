using PMSWPF.Data.Entities;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class BaseRepositories
{
    protected readonly SqlSugarClient _db;

    public BaseRepositories()
    {
        try
        {
            _db = DbContext.GetInstance();
            // _db.DbMaintenance.CreateDatabase();
            // CheckDbTables();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void CheckDbTables()
    {
        if(!_db.DbMaintenance.IsAnyTable<DbNlog>())
            _db.CodeFirst.InitTables<DbNlog>();
       if(!_db.DbMaintenance.IsAnyTable<DbDevice>())
           _db.CodeFirst.InitTables<DbDevice>();
       if(!_db.DbMaintenance.IsAnyTable<DbVariableTable>())
           _db.CodeFirst.InitTables<DbVariableTable>();
       if(!_db.DbMaintenance.IsAnyTable<DbDataVariable>())
           _db.CodeFirst.InitTables<DbDataVariable>();
       if(!_db.DbMaintenance.IsAnyTable<DbS7DataVariable>())
           _db.CodeFirst.InitTables<DbS7DataVariable>();
       if(!_db.DbMaintenance.IsAnyTable<DbUser>())
           _db.CodeFirst.InitTables<DbUser>();
       if(!_db.DbMaintenance.IsAnyTable<DbMqtt>())
           _db.CodeFirst.InitTables<DbMqtt>();
    }
}