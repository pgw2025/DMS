using System.Windows;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using DMS.Core.Enums;
using DMS.Helper;
using DMS.Services;
using DMS.Services.Processors;
using DMS.WPF.ViewModels;
using DMS.WPF.Views;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using DMS.Extensions;
using Microsoft.Extensions.Hosting;
using DMS.Config;
using DMS.Infrastructure.Data;
using DMS.WPF.Helper;
using DMS.WPF.Services;
using DMS.WPF.Services.Processors;
using DMS.WPF.ViewModels.DMS.WPF.ViewModels;
using SqlSugar;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using DMS.Infrastructure.Services;
using DMS.Infrastructure.Interfaces;

namespace DMS;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; }
    public AppSettings Settings { get; private set; }


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

    public new static App Current => (App)System.Windows.Application.Current;
    public IHost Host { get; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        ThemeHelper.InitializeTheme();
        await Host.StartAsync();

        try
        {
            var databaseInitializer = Host.Services.GetRequiredService<DatabaseInitializerService>();
            databaseInitializer.InitializeDataBase();
            await databaseInitializer.InitializeMenu();
            Settings = AppSettings.Load();
            Host.Services.GetRequiredService<GrowlNotificationService>();
            
            // 初始化数据处理链
            var dataProcessingService = Host.Services.GetRequiredService<IDataProcessingService>();
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<CheckValueChangedProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<LoggingProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<MqttPublishProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<UpdateDbVariableProcessor>());
            //dataProcessingService.AddProcessor(Host.Services.GetRequiredService<HistoryProcessor>());
        }
        catch (Exception exception)
        {
            NotificationHelper.ShowError("加载数据时发生错误，如果是连接字符串不正确，可以在设置界面更改：{exception.Message}", exception);
        }
        
        MainWindow = Host.Services.GetRequiredService<MainView>();
        MainWindow.Show();

        // 根据配置启动服务
        // var connectionSettings = DMS.Config.AppSettings.Load();
        // if (connectionSettings.EnableMqttService)
        // {
        //     Host.Services.GetRequiredService<MqttBackgroundService>().StartService();
        // }
        // if (connectionSettings.EnableOpcUaService)
        // {
        //     Host.Services.GetRequiredService<OpcUaBackgroundService>().StartService();
        // }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // 停止服务
        await Host.StopAsync();
        Host.Dispose();
        LogManager.Shutdown();
        base.OnExit(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IDbContext,SqlSugarDbContext>();


        services.AddSingleton<IDeviceDataService, DeviceDataService>();
        services.AddSingleton<NavgatorServices>();
        //services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<GrowlNotificationService>();
        services.AddHostedService<S7BackgroundService>();
        services.AddHostedService<OpcUaBackgroundService>();
        services.AddHostedService<DMS.Infrastructure.Services.MqttBackgroundService>();
        
        
        // 注册 AutoMapper
        services.AddAutoMapper(typeof(App).Assembly);

        // 注册数据处理服务和处理器
        services.AddSingleton<IDataProcessingService, DataProcessingService>();
        services.AddHostedService(provider => (DataProcessingService)provider.GetRequiredService<IDataProcessingService>());
        services.AddSingleton<CheckValueChangedProcessor>();
        services.AddSingleton<LoggingProcessor>();
        services.AddSingleton<UpdateDbVariableProcessor>();
        services.AddSingleton<HistoryProcessor>();
        services.AddSingleton<MqttPublishProcessor>();
        
        // 注册数据仓库
        services.AddSingleton<DeviceRepository>();
        services.AddSingleton<MenuRepository>();
        services.AddSingleton<MqttRepository>();
        services.AddSingleton<UserRepository>();
        services.AddSingleton<VarDataRepository>();
        services.AddSingleton<VarTableRepository>();
        services.AddSingleton<VariableMqttAliasRepository>();
        // 注册视图模型
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DevicesViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddSingleton<SettingViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddSingleton<VariableTableViewModel>();
        //services.AddScoped<MqttServerDetailViewModel>();
        services.AddSingleton<DeviceDetailViewModel>();
        services.AddScoped<MqttsViewModel>();
        //注册View视图
        services.AddSingleton<SettingView>();
        services.AddSingleton<MainView>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<DevicesView>();
        services.AddSingleton<VariableTableView>();
        services.AddScoped<DeviceDetailView>();
        services.AddScoped<MqttsView>();
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

    
}