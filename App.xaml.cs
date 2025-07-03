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
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeLog();
        InitializeServices();
        InitializeDataBase();
        InitializeMenu().Await((e) => { NotificationHelper.ShowMessage($"初始化主菜单失败：{e.Message}"); },
            () => { MessageHelper.SendLoadMessage(LoadTypes.Menu); });

        MainWindow = Services.GetRequiredService<MainView>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 应用程序退出时，确保 NLog 缓冲区被清空
        LogManager.Shutdown();
        base.OnExit(e);
    }

    private void InitializeServices()
    {
        var container = new ServiceCollection();
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
        container.AddTransient<VariableTableViewModel>();
        container.AddSingleton<VariableTableView>();
        container.AddScoped<DeviceDetailViewModel>();
        container.AddScoped<DeviceDetailView>();
        Services = container.BuildServiceProvider();
        // 启动服务
        Services.GetRequiredService<GrowlNotificationService>();
    }

    private void InitializeLog()
    {
        LogManager.Setup().LoadConfigurationFromFile("Config/nlog.config").GetCurrentClassLogger();


        // 捕获未处理的异常并记录
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            if (ex != null)
            {
                // 可以使用一个专用的 Logger 来记录未处理异常
                LogManager.GetLogger("UnhandledExceptionLogger").Fatal($"应用程序发生未处理的异常:{ex}");
            }
        };

        // 捕获 Dispatcher 线程上的未处理异常 (UI 线程)
        this.DispatcherUnhandledException += (sender, args) =>
        {
            LogManager.GetLogger("DispatcherExceptionLogger").Fatal( $"UI 线程发生未处理的异常:{args.Exception}");
            // 标记为已处理，防止应用程序崩溃 (生产环境慎用，可能掩盖问题)
            // args.Handled = true; 
        };

        // 如果您使用 Task (异步方法) 并且没有正确 await，可能会导致异常丢失，
        // 可以通过以下方式捕获 Task 中的异常。
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LogManager.GetLogger("UnobservedTaskExceptionLogger").Fatal( $"异步任务发生未观察到的异常:{args.Exception}");
            // args.SetObserved(); // 标记为已观察，防止进程终止
        };

        
    }

    /// <summary>
    /// 初始化菜单
    /// </summary>
    private async Task InitializeMenu()
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

    private void InitializeDataBase()
    {
        var _db = DbContext.GetInstance();
        _db.DbMaintenance.CreateDatabase();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbNlog>();
        _db.CodeFirst.InitTables<DbDevice>();
        _db.CodeFirst.InitTables<DbVariableTable>();
        _db.CodeFirst.InitTables<DbVariableData>();
        _db.CodeFirst.InitTables<DbVariableS7Data>();
        _db.CodeFirst.InitTables<DbUser>();
        _db.CodeFirst.InitTables<DbMqtt>();
        _db.CodeFirst.InitTables<DbMenu>();
    }
}