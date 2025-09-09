// 文件: DMS.WPF/ViewModels/SplashViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using System;
using System.Threading.Tasks;
using DMS.Application.Services;
using DMS.WPF.Helper;
using DMS.WPF.Interfaces;
using DMS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 启动加载窗口的ViewModel。
/// </summary>
public partial class SplashViewModel : ObservableObject
{
    private readonly ILogger<SplashViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IInitializeService _initializeService;
    private readonly IDataEventService _dataEventService;
    private readonly IAppDataCenterService _appDataCenterService;

    [ObservableProperty]
    private string _loadingMessage = "正在加载...";

    public SplashViewModel(ILogger<SplashViewModel> logger,IServiceProvider serviceProvider, IInitializeService initializeService,IDataEventService dataEventService,
                           IAppDataCenterService appDataCenterService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _initializeService = initializeService;
        _dataEventService = dataEventService;
        this._appDataCenterService = appDataCenterService;
    }

    /// <summary>
    /// 开始执行初始化任务。
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            
            _logger.LogInformation("正在初始化数据库...");
            LoadingMessage = "正在初始化数据库...";
            _initializeService.InitializeTables();
            _initializeService.InitializeMenus();
            LoadingMessage = "正在加载系统配置...";
            await _appDataCenterService.DataLoaderService.LoadAllDataToMemoryAsync();

            // 可以在这里添加加载配置的逻辑
            await Task.Delay(500); // 模拟耗时

            LoadingMessage = "正在连接后台服务...";
            // 可以在这里添加连接服务的逻辑
            await Task.Delay(500); // 模拟耗时

            LoadingMessage = "加载完成，正在启动主界面...";
            await Task.Delay(500);

            // 初始化完成，显示主窗口
            var mainView = App.Current.Services.GetRequiredService<MainView>();
            // 将 MainView 设置为新的主窗口
            App.Current.MainWindow = mainView;
            mainView.Show();
            return true;
        }
        catch (Exception ex)
        {
            // 处理初始化过程中的异常
            LoadingMessage = $"初始化失败: {ex.Message}";
            _logger.LogError(ex,$"初始化失败: {ex}");
            // 在此可以记录日志或显示错误对话框
            return false;
        }
    }
}