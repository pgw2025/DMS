using DMS.Core.Enums;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<InitializeRepository> _logger;

    /// <summary>
    /// 构造函数，注入 SqlSugarDbContext 和 ILogger。
    /// </summary>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public InitializeRepository(SqlSugarDbContext dbContext, ILogger<InitializeRepository> logger)
    {
        _dbContext = dbContext;
        _db = _dbContext.GetInstance();
        _logger = logger;
    }

    /// <summary>
    /// 初始化所有数据库表。
    /// 如果表不存在，则会创建。
    /// </summary>
    public void InitializeTables()
    {
        _db.DbMaintenance.CreateDatabase(); // 创建数据库（如果不存在）
        _db.CodeFirst.InitTables<DbDevice>();
        _db.CodeFirst.InitTables<DbVariableTable>();
        _db.CodeFirst.InitTables<DbVariable>();
        _db.CodeFirst.InitTables<DbVariableHistory>();
        _db.CodeFirst.InitTables<DbUser>();
        _db.CodeFirst.InitTables<DbMqttServer>();
        _db.CodeFirst.InitTables<DbVariableMqttAlias>();
        _db.CodeFirst.InitTables<DbMenu>();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbEmailAccount>();
        _db.CodeFirst.InitTables<DbEmailMessage>();
        _db.CodeFirst.InitTables<DbEmailTemplate>();
        _db.CodeFirst.InitTables<DbEmailLog>();
        _db.CodeFirst.InitTables<DbTriggerDefinition>();
    }

    /// <summary>
    /// 初始化数据库表索引。
    /// 为特定表的列创建唯一索引，以提高查询效率和数据完整性。
    /// </summary>
    public void InitializeTableIndex()
    {
        // 为 DbDevice 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbDevice), new[]
                                                        {
                                                            nameof(DbDevice.Name),
                                                            nameof(DbDevice.OpcUaServerUrl),
                                                        }, true);

        // 为 DbVariable 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbVariable), new[]
                                                          {
                                                              nameof(DbVariable.OpcUaNodeId)
                                                          }, true);
        // 为 DbMqttServer 表创建索引
        _db.DbMaintenance.CreateIndex(nameof(DbMqttServer), new[]
                                                            {
                                                                nameof(DbMqttServer.ServerName)
                                                            }, true);
    }

    /// <summary>
    /// 检查数据库中是否存在指定的表。
    /// </summary>
    /// <param name="tableName">要检查的表名。</param>
    /// <returns>如果表存在则为 true，否则为 false。</returns>
    public bool IsAnyTable(string tableName)
    {
        return _db.DbMaintenance.IsAnyTable(tableName, false);
    }

    /// <summary>
    /// 检查数据库中是否存在指定的索引。
    /// </summary>
    /// <param name="indexName">要检查的索引名。</param>
    /// <returns>如果索引存在则为 true，否则为 false。</returns>
    public bool IsAnyIndex(string indexName)
    {
        return _db.DbMaintenance.IsAnyIndex(indexName);
    }

    /// <summary>
    /// 初始化默认菜单。
    /// 如果数据库中没有菜单，则添加一组默认菜单项。
    /// </summary>
    public void InitializeMenus()
    {
        // 检查数据库中是否已存在菜单数据
        if (_db.Queryable<DbMenu>()
               .Any())
        {
            return; // 如果数据库中已经有菜单，则不进行初始化
        }

        // 创建默认菜单项的 DbMenu 实体列表
        var defaultMenus = new List<DbMenu>
                           {
                               new DbMenu
                               {
                                   Id = 1, Header = "主页", Icon = "\uE80F", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "HomeView", DisplayOrder = 1
                               },
                               new DbMenu
                               {
                                   Id = 2, Header = "设备", Icon = "\uE975", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "DevicesView",
                                   DisplayOrder = 2
                               },
                               new DbMenu
                               {
                                   Id = 3, Header = "数据转换", Icon = "\uF1CB", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "DataTransformView",
                                   DisplayOrder = 3
                               },
                               new DbMenu
                               {
                                   Id = 4, Header = "Mqtt服务器", Icon = "\uE753", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "MqttsView",
                                   DisplayOrder = 4
                               },
                               new DbMenu
                               {
                                   Id = 5, Header = "触发器", Icon = "\uE7BA", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "TriggersView",
                                   DisplayOrder = 5
                               },
                               new DbMenu
                               {
                                   Id = 6, Header = "日志历史", Icon = "\uE7BA", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "LogHistoryView",
                                   DisplayOrder = 6
                               },
                               new DbMenu
                               {
                                   Id = 7, Header = "邮件管理", Icon = "\uE715", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "EmailManagementView",
                                   DisplayOrder = 7
                               },
                               new DbMenu
                               {
                                   Id = 8, Header = "变量历史", Icon = "\uE81C", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "VariableHistoryView",
                                   DisplayOrder = 8
                               },
                               new DbMenu
                               {
                                   Id = 9, Header = "设置", Icon = "\uE713", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "SettingView",
                                   DisplayOrder = 9
                               },
                               new DbMenu
                               {
                                   Id = 10, Header = "关于", Icon = "\uE946", ParentId = 0,
                                   MenuType = MenuType.MainMenu, TargetViewKey = "", DisplayOrder = 10
                               } // 假设有一个AboutView
                           };

        // 批量插入菜单到数据库
        _db.Insertable(defaultMenus)
           .ExecuteCommand();
    }
}