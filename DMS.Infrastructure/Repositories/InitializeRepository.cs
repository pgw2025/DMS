using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

public class InitializeRepository : IInitializeRepository
{
    private readonly SqlSugarDbContext _dbContext;
    private readonly SqlSugarClient _db;

    public InitializeRepository(SqlSugarDbContext dbContext)
    {
        _dbContext = dbContext;
      _db = _dbContext.GetInstance();
    }

    public void InitializeTables()
    {
        _db.DbMaintenance.CreateDatabase();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbDevice>();
        _db.CodeFirst.InitTables<DbVariableTable>();
        _db.CodeFirst.InitTables<DbVariable>();
        _db.CodeFirst.InitTables<DbVariableHistory>();
        _db.CodeFirst.InitTables<DbUser>();
        _db.CodeFirst.InitTables<DbMqttServer>();
        _db.CodeFirst.InitTables<DbVariableMqttAlias>();
        _db.CodeFirst.InitTables<DbMenu>();
    }
    
    public void InitializeTableIndex()
    {
        _db.DbMaintenance.CreateIndex(nameof(DbDevice),new []
                                                 {
                                                     nameof(DbDevice.Name),
                                                     nameof(DbDevice.OpcUaServerUrl),
                                                 },true); 
        
        _db.DbMaintenance.CreateIndex(nameof(DbVariable),new []
                                                 {
                                                     nameof(DbVariable.OpcUaNodeId)
                                                 },true);
        _db.DbMaintenance.CreateIndex(nameof(DbMqttServer),new []
                                                 {
                                                     nameof(DbMqttServer.ServerName)
                                                 },true);
    }

    public bool IsAnyTable(string tableName)
    {
       return  _db.DbMaintenance.IsAnyTable(tableName, false);
    }

    public bool IsAnyIndex(string indexName)
    {
       return  _db.DbMaintenance.IsAnyIndex(indexName);
    }

    public void InitializeMenus()
    {
        var settings = AppSettings.Load();
        if (settings.Menus.Any())
        {
            return ;
        }

        settings.Menus.Add(new MenuBean() { Id=1, Header = "主页",  Icon = "Home", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 2, Header = "设备",  Icon = "Devices3", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 3, Header = "数据转换",  Icon = "ChromeSwitch", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 4, Header = "Mqtt服务器",  Icon = "Cloud", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 5, Header = "设置",  Icon = "Settings", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 6, Header = "关于",  Icon = "Info", ParentId = 0 });

        settings.Save();

        return ;
    }
}