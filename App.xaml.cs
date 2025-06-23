using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using PMSWPF.Data;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Services;
using PMSWPF.ViewModels;
using PMSWPF.Views;
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


        container.AddSingleton<NavgatorServices>();
        container.AddSingleton<DevicesRepositories>();
        container.AddSingleton<IDeviceDialogService, DeviceDialogService>();
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

        MainWindow = Services.GetRequiredService<MainView>();
        MainWindow.Show();
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
    }
}