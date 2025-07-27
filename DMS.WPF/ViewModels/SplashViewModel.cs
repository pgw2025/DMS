// 文件: DMS.WPF/ViewModels/SplashViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using System;
using System.Threading.Tasks;
using DMS.Application.Services;
using DMS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 启动加载窗口的ViewModel。
/// </summary>
public partial class SplashViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInitializeService _initializeService;
    private readonly DataServices _dataServices;

    [ObservableProperty]
    private string _loadingMessage = "正在加载...";

    public SplashViewModel(IServiceProvider serviceProvider, IInitializeService initializeService,DataServices dataServices)
    {
        _serviceProvider = serviceProvider;
        _initializeService = initializeService;
        _dataServices = dataServices;
    }

    /// <summary>
    /// 开始执行初始化任务。
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            LoadingMessage = "正在初始化数据库...";
             _initializeService.InitializeTables();
             _initializeService.InitializeMenus();

            LoadingMessage = "正在加载系统配置...";
            await _dataServices.LoadDevices();
            await _dataServices.LoadVariableTables();
            await _dataServices.LoadVariables();
            await _dataServices.LoadMenus();
            
            _dataServices.AssociateVariableTablesToDevices();
            _dataServices.AssociateVariablesToVariableTables();
            
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
            Console.WriteLine($"初始化失败: {ex}");
            // 在此可以记录日志或显示错误对话框
            return false;
        }
    }
}
