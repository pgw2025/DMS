using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
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
        container.AddSingleton<IDialogService,DialogService>();
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
        // InitMenu();

        MainWindow = Services.GetRequiredService<MainView>();
        MainWindow.Show();
    }

    private void InitMenu()
    {
        using (var db = DbContext.GetInstance())
        {
            List<DbMenu> items = new List<DbMenu>();
            items.Add(new DbMenu() {  Name = "主页", Icon = SegoeFluentIcons.Home.Glyph, ParentId = 0});
            items.Add(new DbMenu() {  Name = "设备", Icon = SegoeFluentIcons.Devices.Glyph, ParentId = 0});
            items.Add(new DbMenu() {  Name = "数据转换", Icon = SegoeFluentIcons.Move.Glyph, ParentId = 0});
            items.Add(new DbMenu() {  Name = "设置", Icon = SegoeFluentIcons.Settings.Glyph, ParentId = 0});
            items.Add(new DbMenu() {  Name = "关于", Icon = SegoeFluentIcons.Info.Glyph, ParentId = 0});
            db.Insertable<DbMenu>(items).ExecuteCommand();
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