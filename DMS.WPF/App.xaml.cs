using System.Windows;
using AutoMapper.Internal;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Enums;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Helper;
using DMS.Services.Processors;
using DMS.WPF.ViewModels;
using DMS.WPF.Views;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using DMS.Extensions;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Repositories;
using DMS.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using DMS.WPF.Helper;
using DMS.WPF.Services;
using DMS.WPF.Services.Processors;
using DMS.WPF.ViewModels.Dialogs;
using DataProcessingService = DMS.Services.DataProcessingService;
using IDataProcessingService = DMS.Services.IDataProcessingService;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DMS;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; }
    // public AppSettings Settings { get; private set; }


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
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        ThemeHelper.InitializeTheme();
        await Host.StartAsync();

        try
        {
            // var databaseInitializer = Host.Services.GetRequiredService<DatabaseInitializerService>();
            // databaseInitializer.InitializeDataBase();
            // await databaseInitializer.InitializeMenu();
            // Settings = AppSettings.Load();
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
        
        var splashWindow = Host.Services.GetRequiredService<SplashWindow>();
        splashWindow.Show();

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
        // services.AddTransient<IDbContext,SqlSugarDbContext>();
        //
        //
        // services.AddSingleton<IDeviceDataService, DeviceDataService>();
        // services.AddSingleton<NavgatorServices>();
        // //services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<GrowlNotificationService>();
        // services.AddHostedService<S7BackgroundService>();
        // services.AddHostedService<OpcUaBackgroundService>();
        // services.AddHostedService<DMS.Infrastructure.Services.MqttBackgroundService>();
        //
        
        // --- 核心配置 ---
        services.AddAutoMapper(cfg =>
        {
            // 最终解决方案：根据异常信息的建议，设置此标记以忽略重复的Profile加载。
            // 注意：此属性位于 Internal() 方法下。
            cfg.Internal().AllowAdditiveTypeMapCreation = true;

            cfg.AddProfile(new DMS.Application.Profiles.MappingProfile());
            cfg.AddProfile(new DMS.Infrastructure.Profiles.MappingProfile());
            cfg.AddProfile(new DMS.WPF.Profiles.MappingProfile());
        });

        // 注册数据处理服务和处理器
        services.AddSingleton<IDataProcessingService, DataProcessingService>();
        services.AddHostedService(provider => (DataProcessingService)provider.GetRequiredService<IDataProcessingService>());
        services.AddSingleton<CheckValueChangedProcessor>();
        services.AddSingleton<LoggingProcessor>();
        services.AddSingleton<UpdateDbVariableProcessor>();
        services.AddSingleton<HistoryProcessor>();
        services.AddSingleton<MqttPublishProcessor>();
        
        // 注册Core中的仓库
        services.AddSingleton<AppSettings>();
        // services.AddSingleton<SqlSugarDbContext>();
        // 2. 配置数据库上下文 (在测试中通常使用单例)
        services.AddSingleton<SqlSugarDbContext>(_ =>
        {
            var appSettings = new AppSettings { Database = { Database = "dms_test" } };
            return new SqlSugarDbContext(appSettings);
        });
        
        services.AddSingleton<IInitializeRepository, InitializeRepository>();
        services.AddSingleton<IRepositoryManager, RepositoryManager>();
        services.AddSingleton<IExcelService, ExcelService>();
        
        
        // 注册App服务
        services.AddSingleton<IInitializeService,InitializeService>();
        services.AddSingleton<IDeviceAppService,DeviceAppService>();
        services.AddSingleton<IVariableAppService,VariableAppService>();
        services.AddSingleton<IVariableTableAppService,VariableTableAppService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IDialogService, DialogService>();
        //注册WPF中的服务
        services.AddSingleton<DataServices>();
        // 注册视图模型
        services.AddSingleton<SplashViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DevicesViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddSingleton<SettingViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddTransient<VariableTableViewModel>();
        //services.AddScoped<MqttServerDetailViewModel>();
        services.AddSingleton<DeviceDetailViewModel>();
        services.AddSingleton<MqttsViewModel>();
        // 注册对话框模型
        services.AddTransient<ImportExcelDialogViewModel>();
        services.AddTransient<VariableDialogViewModel>();
        // 注册对话框
        services.AddSingleton<DevicesView>();
        //注册View视图
        services.AddSingleton<SplashWindow>();
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
        // loggingBuilder.AddNLog();

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