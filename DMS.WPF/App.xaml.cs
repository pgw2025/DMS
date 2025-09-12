using System.Windows;
using AutoMapper;
using AutoMapper.Internal;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Application.Services.Processors;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Interfaces.Services;
using DMS.Infrastructure.Configuration;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Repositories;
using DMS.Infrastructure.Services;
using DMS.WPF.Helper;
using DMS.WPF.Interfaces;
using DMS.WPF.Logging;
using DMS.WPF.Services;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DMS.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; }
    public new static App Current => (App)System.Windows.Application.Current;
    public IHost Host { get; }
    private readonly ILogger<App> _logger;

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
        _logger = Host.Services.GetRequiredService<ILogger<App>>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        ThemeHelper.InitializeTheme();
        await Host.StartAsync();

        try
        {
            Host.Services.GetRequiredService<GrowlNotificationService>();
            
            // 初始化数据处理链
            var dataProcessingService = Host.Services.GetRequiredService<IDataProcessingService>();
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<CheckValueChangedProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<LoggingProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<HistoryProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<MqttPublishProcessor>());
            dataProcessingService.AddProcessor(Host.Services.GetRequiredService<UpdateDbVariableProcessor>());
        }
        catch (Exception exception)
        {
            var notificationService = Host.Services.GetRequiredService<INotificationService>();
            notificationService.ShowError($"加载数据时发生错误，如果是连接字符串不正确，可以在设置界面更改：{exception.Message}", exception);
        }
        
        var splashWindow = Host.Services.GetRequiredService<SplashWindow>();
        splashWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _logger.LogInformation("应用程序正在关闭，开始清理资源...");

        try
        {
            // 获取服务管理器
            var opcUaServiceManager = Host.Services.GetService<IOpcUaServiceManager>();
            var s7ServiceManager = Host.Services.GetService<IS7ServiceManager>();
            var mqttServiceManager = Host.Services.GetService<IMqttServiceManager>();

            // 优雅地关闭OPC UA服务
            if (opcUaServiceManager != null)
            {
                _logger.LogInformation("正在关闭OPC UA服务...");
                opcUaServiceManager.Dispose();
                _logger.LogInformation("OPC UA服务已关闭");
            }

            // 优雅地关闭S7服务
            if (s7ServiceManager != null)
            {
                _logger.LogInformation("正在关闭S7服务...");
                s7ServiceManager.Dispose();
                _logger.LogInformation("S7服务已关闭");
            }

            // 优雅地关闭MQTT服务
            if (mqttServiceManager != null)
            {
                _logger.LogInformation("正在关闭MQTT服务...");
                mqttServiceManager.Dispose();
                _logger.LogInformation("MQTT服务已关闭");
            }

            // 停止后台服务
            _logger.LogInformation("正在停止后台服务...");
            await Host.StopAsync();
            _logger.LogInformation("后台服务已停止");

            // 释放Host资源
            _logger.LogInformation("正在释放Host资源...");
            Host.Dispose();
            _logger.LogInformation("Host资源已释放");

            // 关闭NLog
            _logger.LogInformation("正在关闭NLog...");
            LogManager.Shutdown();
            _logger.LogInformation("NLog已关闭");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭应用程序时发生错误: {ErrorMessage}", ex.Message);
        }

        _logger.LogInformation("应用程序已完全关闭");
        base.OnExit(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 注册NLogLogger作为Microsoft.Extensions.Logging.ILogger的实现
        services.AddSingleton<ILogger, NLogLogger>();
        services.AddSingleton<ILoggerFactory, NLogLoggerFactory>();
        services.AddSingleton<GrowlNotificationService>();
        services.AddSingleton<INotificationService, NotificationService>();
        
        // 注册核心服务
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
        // services.AddHostedService<OpcUaBackgroundService>();
        //注册OpcUa相关的服务
        services.Configure<OpcUaServiceOptions>(options => { });
        services.AddSingleton<IOpcUaServiceManager, OpcUaServiceManager>();
        services.AddHostedService<OptimizedOpcUaBackgroundService>();
        // 注册S7相关的服务
        services.AddSingleton<IS7ServiceFactory, S7ServiceFactory>();
        services.AddSingleton<IS7ServiceManager, S7ServiceManager>();
        services.AddSingleton<IChannelBus, ChannelBus>();
        services.AddSingleton<IMessenger, Messenger>();
        services.AddHostedService<OptimizedS7BackgroundService>();
        
        
        services.AddSingleton<IDataProcessingService, DataProcessingService>();
        services.AddHostedService(provider => (DataProcessingService)provider.GetRequiredService<IDataProcessingService>());
        services.AddSingleton<CheckValueChangedProcessor>();
        services.AddSingleton<LoggingProcessor>();
        services.AddSingleton<UpdateDbVariableProcessor>();
        services.AddSingleton<HistoryProcessor>();
        services.AddSingleton<MqttPublishProcessor>();
        
        // 注册Core中的仓库
        services.AddSingleton<AppSettings>();
        services.AddSingleton<SqlSugarDbContext>(_ =>
        {
            var appSettings = new AppSettings { Database = { Database = "dms_test" } };
            return new SqlSugarDbContext(appSettings);
        });
        
        services.AddSingleton<IInitializeRepository, InitializeRepository>();
        services.AddSingleton<IDeviceRepository, DeviceRepository>();
        services.AddSingleton<IVariableTableRepository, VariableTableRepository>();
        services.AddSingleton<IVariableRepository, VariableRepository>();
        services.AddSingleton<IMqttServerRepository, MqttServerRepository>();
        services.AddSingleton<IVariableMqttAliasRepository, VariableMqttAliasRepository>();
        services.AddSingleton<IMenuRepository, MenuRepository>();
        services.AddSingleton<IVariableHistoryRepository, VariableHistoryRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<INlogRepository, NlogRepository>();
        services.AddSingleton<IRepositoryManager, RepositoryManager>();
        services.AddSingleton<IExcelService, ExcelService>();

        services.AddTransient<IOpcUaService, OpcUaService>();
        services.AddTransient<IMqttService, MqttService>();
        services.AddTransient<IMqttServiceFactory, MqttServiceFactory>();
        
        // 注册App服务\r\n
        services.AddSingleton<IInitializeService, InitializeService>();
        services.AddSingleton<IDeviceAppService, DeviceAppService>();
        services.AddSingleton<IVariableAppService, VariableAppService>();
        services.AddSingleton<IHistoryAppService, HistoryAppService>();
        services.AddSingleton<IVariableTableAppService, VariableTableAppService>();
        services.AddSingleton<IMenuService, MenuService>();       
        services.AddSingleton<IAppDataCenterService, AppDataCenterService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IAppDataStorageService, AppDataStorageService>();
        services.AddSingleton<IDeviceManagementService, DeviceManagementService>();
        services.AddSingleton<IVariableTableManagementService, VariableTableManagementService>();
        services.AddSingleton<IVariableManagementService, VariableManagementService>();
        services.AddSingleton<IMenuManagementService, MenuManagementService>();
        services.AddSingleton<IMqttManagementService, MqttManagementService>();
        services.AddSingleton<ILogManagementService, LogManagementService>();
        
        services.AddSingleton<IDataLoaderService, DataLoaderService>();
        services.AddSingleton<INlogAppService, NlogAppService>();
        
        // 注册MQTT服务管理器
        services.AddSingleton<IMqttServiceManager, MqttServiceManager>();
        services.AddSingleton<IMqttAliasAppService, MqttAliasAppService>();
        services.AddHostedService<MqttBackgroundService>();
        
        // 注册WPF中的服务
        services.AddSingleton<IMqttAppService, MqttAppService>();
        
        // 注册新的数据服务
        services.AddSingleton<IDeviceDataService, DeviceDataService>();
        services.AddSingleton<IVariableDataService, VariableDataService>();
        services.AddSingleton<IVariableTableDataService, VariableTableDataService>();
        services.AddSingleton<IMenuDataService, MenuDataService>();
        services.AddSingleton<IMqttDataService, MqttDataService>();
        services.AddSingleton<ILogDataService, LogDataService>();
        services.AddSingleton<IDataEventService, DataEventService>();
        services.AddSingleton<IDataStorageService, DataStorageService>();
        
        // 注册主数据服务
        services.AddSingleton<IWPFDataService, WPFDataService>();
        
        // 保留原DataServices以保证现有代码兼容性（可选，建议逐步移除）
        // services.AddSingleton<DataServices>(provider => 
        //     new DataServices(
        //         provider.GetRequiredService<IMapper>(),
        //         provider.GetRequiredService<IAppDataCenterService>(),
        //         provider.GetRequiredService<IMqttAppService>()
        //     )
        // );
        

        

        
        // 注册视图模型
        services.AddSingleton<SplashViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DevicesViewModel>();
        services.AddSingleton<DataTransformViewModel>();
        services.AddSingleton<SettingViewModel>();
        services.AddSingleton<LogHistoryViewModel>();
        services.AddTransient<VariableTableViewModel>();
        services.AddSingleton<DeviceDetailViewModel>();
        services.AddSingleton<MqttsViewModel>();
        services.AddSingleton<LogHistoryViewModel>();
        services.AddScoped<MqttServerDetailViewModel>();
        services.AddSingleton<VariableHistoryViewModel>();
        
        // 注册对话框视图模型
        services.AddTransient<DeviceDialogViewModel>();
        services.AddTransient<ConfirmDialogViewModel>();
        services.AddTransient<VariableTableDialogViewModel>();
        services.AddTransient<ImportExcelDialogViewModel>();
        services.AddTransient<ImportOpcUaDialogViewModel>();
        services.AddTransient<VariableDialogViewModel>();
        services.AddTransient<PollLevelDialogViewModel>();
        services.AddTransient<IsActiveDialogViewModel>();
        services.AddTransient<MqttDialogViewModel>();
        services.AddTransient<MqttSelectionDialogViewModel>();
        services.AddTransient<MqttAliasBatchEditDialogViewModel>();
        services.AddTransient<HistorySettingsDialogViewModel>();
        services.AddTransient<AlarmSettingsDialogViewModel>();
        
        // 注册View视图
        services.AddSingleton<SplashWindow>();
        services.AddSingleton<SettingView>();
        services.AddSingleton<MainView>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<DevicesView>();
        services.AddSingleton<VariableTableView>();
        services.AddSingleton<VariableHistoryView>();
        services.AddSingleton<LogHistoryView>();
        services.AddScoped<DeviceDetailView>();
        services.AddScoped<MqttsView>();
    }

    private void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        LogManager.Setup().LoadConfigurationFromFile("Configurations/nlog.config");
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        // loggingBuilder.addn; // 添加NLog作为日志提供者

        // // 捕获未处理的异常并记录
        // AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        // {
        //     var ex = args.ExceptionObject as Exception;
        //     if (ex != null)
        //     {
        //         // 可以使用一个专用的 Logger 来记录未处理异常
        //         LogManager.GetLogger("UnhandledExceptionLogger")
        //             .Fatal($"应用程序发生未处理的异常:{ex}");
        //     }
        // };
        //
        // // 捕获 Dispatcher 线程上的未处理异常 (UI 线程)
        // this.DispatcherUnhandledException += (sender, args) =>
        // {
        //     LogManager.GetLogger("DispatcherExceptionLogger")
        //         .Fatal($"UI 线程发生未处理的异常:{args.Exception}");
        //     // 标记为已处理，防止应用程序崩溃 (生产环境慎用，可能掩盖问题)
        //     // args.Handled = true; 
        // };
        //
        // // 如果您使用 Task (异步方法) 并且没有正确 await，可能会导致异常丢失，
        // // 可以通过以下方式捕获 Task 中的异常。
        // TaskScheduler.UnobservedTaskException += (sender, args) =>
        // {
        //     LogManager.GetLogger("UnobservedTaskExceptionLogger")
        //         .Fatal($"异步任务发生未观察到的异常:{args.Exception}");
        //     // args.SetObserved(); // 标记为已观察，防止进程终止
        // };
    }
}