// 文件: DMS.WPF/ViewModels/SplashViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using System;
using System.Threading.Tasks;
using DMS.Application.Services;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 启动加载窗口的ViewModel。
/// </summary>
public partial class SplashViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInitializeService _initializeService;

    [ObservableProperty]
    private string _loadingMessage = "正在加载...";

    public SplashViewModel(IServiceProvider serviceProvider, IInitializeService initializeService)
    {
        _serviceProvider = serviceProvider;
        _initializeService = initializeService;
    }

    /// <summary>
    /// 开始执行初始化任务。
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            LoadingMessage = "正在初始化数据库...";
             _initializeService.InitializeTables();

            LoadingMessage = "正在加载系统配置...";
            // 可以在这里添加加载配置的逻辑
            await Task.Delay(1500); // 模拟耗时

            LoadingMessage = "正在连接后台服务...";
            // 可以在这里添加连接服务的逻辑
            await Task.Delay(1500); // 模拟耗时

            LoadingMessage = "加载完成，正在启动主界面...";
            await Task.Delay(1500);

            // 初始化完成，显示主窗口
            var navigationService = (INavigationService)_serviceProvider.GetService(typeof(INavigationService));
            await navigationService.ShowMainWindowAsync();
        }
        catch (Exception ex)
        {
            // 处理初始化过程中的异常
            LoadingMessage = $"初始化失败: {ex.Message}";
            // 在此可以记录日志或显示错误对话框
        }
    }
}
