using DMS.Config;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class DatabaseInitializerService : DMS.Infrastructure.Interfaces.IDatabaseService
    {
        private readonly SqlSugarClient _db;

        public DatabaseInitializerService(SqlSugarDbContext dbContext)
        {
            _db = dbContext.GetInstance();
        }

        public void InitializeDataBase()
        {
            _db.DbMaintenance.CreateDatabase();
            _db.CodeFirst.InitTables<DbNlog>();
            _db.CodeFirst.InitTables<DbDevice>();
            _db.CodeFirst.InitTables<DbVariableTable>();
            _db.CodeFirst.InitTables<DbVariable>();
            _db.CodeFirst.InitTables<DbVariableHistory>();
            _db.CodeFirst.InitTables<DbUser>();
            _db.CodeFirst.InitTables<DbMqtt>();
            _db.CodeFirst.InitTables<DbVariableMqtt>();
            _db.CodeFirst.InitTables<DbMenu>();
        }

        public Task InitializeMenu()
        {
            var settings = AppSettings.Load();
            if (settings.Menus.Any())
            {
                return Task.CompletedTask;
            }

            settings.Menus.Add(new MenuBean() { Id=1, Name = "主页", Type = MenuType.MainMenu, Icon = "Home", ParentId = 0 });
            settings.Menus.Add(new MenuBean() { Id = 2, Name = "设备", Type = MenuType.MainMenu, Icon = "Devices3", ParentId = 0 });
            settings.Menus.Add(new MenuBean() { Id = 3, Name = "数据转换", Type = MenuType.MainMenu, Icon = "ChromeSwitch", ParentId = 0 });
            settings.Menus.Add(new MenuBean() { Id = 4, Name = "Mqtt服务器", Type = MenuType.MainMenu, Icon = "Cloud", ParentId = 0 });
            settings.Menus.Add(new MenuBean() { Id = 5, Name = "设置", Type = MenuType.MainMenu, Icon = "Settings", ParentId = 0 });
            settings.Menus.Add(new MenuBean() { Id = 6, Name = "关于", Type = MenuType.MainMenu, Icon = "Info", ParentId = 0 });

            settings.Save();

            return Task.CompletedTask;
        }
    }
}
