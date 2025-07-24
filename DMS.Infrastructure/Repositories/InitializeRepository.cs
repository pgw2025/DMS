using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 初始化仓储实现类，负责数据库表和索引的初始化以及默认菜单的创建。
/// 实现 <see cref="IInitializeRepository"/> 接口。
/// </summary>
public class InitializeRepository : IInitializeRepository
{
    private readonly SqlSugarDbContext _dbContext;
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 构造函数，注入 SqlSugarDbContext。
    /// </summary>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    public InitializeRepository(SqlSugarDbContext dbContext)
    {
        _dbContext = dbContext;
      _db = _dbContext.GetInstance();
    }

    /// <summary>
    /// 初始化所有数据库表。
    /// 如果表不存在，则会创建。
    /// </summary>
    public void InitializeTables()
    {
        _db.DbMaintenance.CreateDatabase(); // 创建数据库（如果不存在）
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
    
    /// <summary>
    /// 初始化数据库表索引。
    /// 为特定表的列创建唯一索引，以提高查询效率和数据完整性。
    /// </summary>
    public void InitializeTableIndex()
    {
        // 为 DbDevice 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbDevice),new []
                                                 {
                                                     nameof(DbDevice.Name),
                                                     nameof(DbDevice.OpcUaServerUrl),
                                                 },true); 
        
        // 为 DbVariable 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbVariable),new []
                                                 {
                                                     nameof(DbVariable.OpcUaNodeId)
                                                 },true);
        // 为 DbMqttServer 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbMqttServer),new []
                                                 {
                                                     nameof(DbMqttServer.ServerName)
                                                 },true);
    }

    /// <summary>
    /// 检查数据库中是否存在指定的表。
    /// </summary>
    /// <param name="tableName">要检查的表名。</param>
    /// <returns>如果表存在则为 true，否则为 false。</returns>
    public bool IsAnyTable(string tableName)
    {
       return  _db.DbMaintenance.IsAnyTable(tableName, false);
    }

    /// <summary>
    /// 检查数据库中是否存在指定的索引。
    /// </summary>
    /// <param name="indexName">要检查的索引名。</param>
    /// <returns>如果索引存在则为 true，否则为 false。</returns>
    public bool IsAnyIndex(string indexName)
    {
       return  _db.DbMaintenance.IsAnyIndex(indexName);
    }

    /// <summary>
    /// 初始化默认菜单。
    /// 如果配置文件中没有菜单，则添加一组默认菜单项。
    /// </summary>
    public void InitializeMenus()
    {
        var settings = AppSettings.Load();
        if (settings.Menus.Any())
        {
            return ; // 如果已经有菜单，则不进行初始化
        }

        // 添加默认菜单项
        settings.Menus.Add(new MenuBean() { Id=1, Header = "主页",  Icon = "Home", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 2, Header = "设备",  Icon = "Devices3", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 3, Header = "数据转换",  Icon = "ChromeSwitch", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 4, Header = "Mqtt服务器",  Icon = "Cloud", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 5, Header = "设置",  Icon = "Settings", ParentId = 0 });
        settings.Menus.Add(new MenuBean() { Id = 6, Header = "关于",  Icon = "Info", ParentId = 0 });

        settings.Save(); // 保存菜单到配置文件

        return ;
    }
}