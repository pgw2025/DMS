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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        if (!_db.DbMaintenance.IsAnyTable<DbNlog>())
            _db.CodeFirst.InitTables<DbNlog>();
        if (!_db.DbMaintenance.IsAnyTable<DbDevice>())
            _db.CodeFirst.InitTables<DbDevice>();
        if (!_db.DbMaintenance.IsAnyTable<DbVariableTable>())
            _db.CodeFirst.InitTables<DbVariableTable>();
        if (!_db.DbMaintenance.IsAnyTable<DbDataVariable>())
            _db.CodeFirst.InitTables<DbDataVariable>();
        if (!_db.DbMaintenance.IsAnyTable<DbS7DataVariable>())
            _db.CodeFirst.InitTables<DbS7DataVariable>();
        if (!_db.DbMaintenance.IsAnyTable<DbUser>())
            _db.CodeFirst.InitTables<DbUser>();
        if (!_db.DbMaintenance.IsAnyTable<DbMqtt>())
            _db.CodeFirst.InitTables<DbMqtt>();
        stopwatch.Stop();
        // Assuming NLog is available and configured for BaseRepositories
        // If not, you might need to add a Logger field similar to DeviceRepository
        // For now, I'll assume it's available or will be added.
        // LogManager.GetCurrentClassLogger().Info($"检查数据库表耗时：{stopwatch.ElapsedMilliseconds}ms");
    }
}