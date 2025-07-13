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
using Microsoft.Extensions.Hosting;
using PMSWPF.Config;
using PMSWPF.ViewModels.Dialogs;
using SqlSugar;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PMSWPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>
                        {
                            ConfigureServices(services);
                        })
                        .ConfigureLogging(loggingBuilder =>
                        {
                            ConfigureLogging(loggingBuilder);
                        })
                        .Build();
        Services = Host.Services;
    }

    public new static App Current => (App)Application.Current;
    public IHost Host { get; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        ThemeHelper.InitializeTheme();
        await Host.StartAsync();

        try
        {
            InitializeDataBase();
            InitializeMenu()
                .Await((e) => { NotificationHelper.ShowError($"初始化主菜单失败：{e.Message}", e); },
                       () => { MessageHelper.SendLoadMessage(LoadTypes.Menu); });
            Host.Services.GetRequiredService<GrowlNotificationService>();
        }
        catch (Exception exception)
        {
            NotificationHelper.ShowError("加载数据时发生错误，如果是连接字符串不正确，可以在设置界面更改：{exception.Message}", exception);
        }
        
        MainWindow = Host.Services.GetRequiredService<MainView>();
        MainWindow.Show();

        // 根据配置启动服务
        var connectionSettings = PMSWPF.Config.ConnectionSettings.Load();
        if (connectionSettings.EnableS7Service)
        {
            Host.Services.GetRequiredService<S7BackgroundService>().StartService();
        }
        if (connectionSettings.EnableMqttService)
        {
            Host.Services.GetRequiredService<MqttBackgroundService>().StartService();
        }
        if (connectionSettings.EnableOpcUaService)
        {
            Host.Services.GetRequiredService<OpcUaBackgroundService>().StartService();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // 停止服务
        Host.Services.GetRequiredService<S7BackgroundService>().StopService();
        Host.Services.GetRequiredService<MqttBackgroundService>().StopService();
        Host.Services.GetRequiredService<OpcUaBackgroundService>().StopService();

        await Host.StopAsync();
        Host.Dispose();
        LogManager.Shutdown();
        base.OnExit(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DataServices>();
        services.AddSingleton<NavgatorServices>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<GrowlNotificationService>();
        services.AddSingleton<S7BackgroundService>();
        services.AddSingleton<MqttBackgroundService>();
        services.AddSingleton<OpcUaBackgroundService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DevicesViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddSingleton<SettingViewModel>();
        services.AddSingleton<SettingView>();
        services.AddSingleton<MainView>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<DevicesView>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddTransient<VariableTableViewModel>();
        services.AddSingleton<VariableTableView>();
        services.AddScoped<DeviceDetailViewModel>();
        services.AddScoped<DeviceDetailView>();
        services.AddScoped<MqttsViewModel>();
        services.AddScoped<MqttsView>();
        services.AddScoped<MqttServerDetailViewModel>();
    }

    private void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        LogManager.Setup().LoadConfigurationFromFile("Config/nlog.config");
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        loggingBuilder.AddNLog();

        // 捕获未处理的异常并记录
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            if (ex != null)
            {
                // 可以使用一个专用的 Logger 来记录未处理异常
                LogManager.GetLogger("UnhandledExceptionLogger")
                          .Fatal($"应用程序发生未处理的异常:{ex}");
            }
        };

        // 捕获 Dispatcher 线程上的未处理异常 (UI 线程)
        this.DispatcherUnhandledException += (sender, args) =>
        {
            LogManager.GetLogger("DispatcherExceptionLogger")
                      .Fatal($"UI 线程发生未处理的异常:{args.Exception}");
            // 标记为已处理，防止应用程序崩溃 (生产环境慎用，可能掩盖问题)
            // args.Handled = true; 
        };

        // 如果您使用 Task (异步方法) 并且没有正确 await，可能会导致异常丢失，
        // 可以通过以下方式捕获 Task 中的异常。
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LogManager.GetLogger("UnobservedTaskExceptionLogger")
                      .Fatal($"异步任务发生未观察到的异常:{args.Exception}");
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
                             {
                                 Name = "设备", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Devices3.Glyph,
                                 ParentId = 0
                             };
            var dataTransfromMenu = new DbMenu()
                                    {
                                        Name = "数据转换", Type = MenuType.MainMenu,
                                        Icon = SegoeFluentIcons.ChromeSwitch.Glyph, ParentId = 0
                                    };
            var mqttMenu = new DbMenu()
                           {
                               Name = "Mqtt服务器", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Cloud.Glyph,
                               ParentId = 0
                           };

            var settingMenu = new DbMenu()
                              {
                                  Name = "设置", Type = MenuType.MainMenu, Icon = SegoeFluentIcons.Settings.Glyph,
                                  ParentId = 0
                              };
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
        var homeMenuExist = await db.Queryable<DbMenu>()
                                    .FirstAsync(dm => dm.Name == menu.Name);
        if (homeMenuExist == null)
        {
            await db.Insertable<DbMenu>(menu)
                    .ExecuteCommandAsync();
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
        _db.CodeFirst.InitTables<DbVariableDataMqtt>();
        _db.CodeFirst.InitTables<DbMenu>();
    }
}