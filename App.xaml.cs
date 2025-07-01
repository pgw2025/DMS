using System.Windows;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using PMSWPF.Data;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Services;
using PMSWPF.ViewModels;
using PMSWPF.Views;
using SqlSugar;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PMSWPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        var container = new ServiceCollection();

        var nlog = LogManager.Setup().LoadConfigurationFromFile("Config/nlog.config").GetCurrentClassLogger();

        container.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog();
        });


        container.AddSingleton<DataServices>();
        container.AddSingleton<NavgatorServices>();
        container.AddSingleton<IDialogService, DialogService>();
        container.AddSingleton<GrowlNotificationService>();
        container.AddSingleton<MainViewModel>();
        container.AddSingleton<HomeViewModel>();
        container.AddSingleton<DevicesViewModel>();
        container.AddSingleton<DataTransformViewModel>();
        container.AddSingleton<SettingViewModel>();
        container.AddSingleton<SettingView>();
        container.AddSingleton<MainView>();
        container.AddSingleton<HomeView>();
        container.AddSingleton<DevicesView>();
        container.AddSingleton<DataTransformViewModel>();
        container.AddSingleton<VariableTableViewModel>();
        container.AddSingleton<VariableTableView>();
        container.AddScoped<DeviceDetailViewModel>();
        container.AddScoped<DeviceDetailView>();
        Services = container.BuildServiceProvider();
        // 启动服务
        Services.GetRequiredService<GrowlNotificationService>();
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitDB();
        InitMenu().Await((e) =>
        {
            NotificationHelper.ShowMessage($"初始化主菜单失败：{e.Message}");
        }, () =>
        {
            MessageHelper.SendLoadMessage(LoadTypes.Menu);
        });

        MainWindow = Services.GetRequiredService<MainView>();
        MainWindow.Show();
    }

    private async Task InitMenu()
    {
        using (var db = DbContext.GetInstance())
        {
            var homeMenu = new DbMenu()
                { Name = "主页", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Home.Glyph, ParentId = 0 };

            var deviceMenu = new DbMenu()
                { Name = "设备", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Devices3.Glyph, ParentId = 0 };
            var dataTransfromMenu = new DbMenu()
                { Name = "数据转换", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.ChromeSwitch.Glyph, ParentId = 0 };
            var mqttMenu = new DbMenu()
                { Name = "Mqtt服务器", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Cloud.Glyph, ParentId = 0 };
  
            var settingMenu = new DbMenu()
                { Name = "设置", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Settings.Glyph, ParentId = 0 };
            var aboutMenu = new DbMenu()
                { Name = "关于", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Info.Glyph, ParentId = 0 };
            await CheckMainMenuExist(db, homeMenu);
            await CheckMainMenuExist(db, deviceMenu);
            await CheckMainMenuExist(db, dataTransfromMenu);
            await CheckMainMenuExist(db, mqttMenu);
            await CheckMainMenuExist(db, settingMenu);
            await CheckMainMenuExist(db, aboutMenu);
        }
    }

    private static async Task CheckMainMenuExist(SqlSugarClient db, DbMenu menu)
    {
        var homeMenuExist = await db.Queryable<DbMenu>().FirstAsync(dm => dm.Name == menu.Name);
        if (homeMenuExist == null)
        {
            await db.Insertable<DbMenu>(menu).ExecuteCommandAsync();
        }
    }

    private void InitDB()
    {
        var _db = DbContext.GetInstance();
        _db.DbMaintenance.CreateDatabase();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbDevice>();
        _db.CodeFirst.InitTables<DbVariableTable>();
        _db.CodeFirst.InitTables<DbDataVariable>();
        _db.CodeFirst.InitTables<DbS7DataVariable>();
        _db.CodeFirst.InitTables<DbUser>();
        _db.CodeFirst.InitTables<DbMqtt>();
        _db.CodeFirst.InitTables<DbMenu>();
    }
}