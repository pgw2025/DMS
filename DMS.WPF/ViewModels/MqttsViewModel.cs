using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using System.Collections.ObjectModel;

namespace DMS.WPF.ViewModels;

/// <summary>
/// MQTT服务器管理视图模型，负责MQTT服务器的增删改查操作。
/// </summary>
public partial class MqttsViewModel : ViewModelBase
{
    private readonly IWPFDataService _wpfDataService;
    private readonly IMqttAppService _mqttAppService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 设备列表。
    /// </summary>
    [ObservableProperty]
    private INotifyCollectionChangedSynchronizedViewList<MqttServerItemViewModel> _mqttServeise;
    

    /// <summary>
    /// 当前选中的MQTT服务器。
    /// </summary>
    [ObservableProperty]
    private MqttServerItemViewModel _selectedMqtt;

    private readonly ILogger<MqttsViewModel> _logger;

    public MqttsViewModel(
        ILogger<MqttsViewModel> logger, 
        IDialogService dialogService, 
        IWPFDataService wpfDataService,
        IMqttAppService mqttAppService,
        IDataStorageService dataStorageService,
        IMapper mapper,
        INavigationService navigationService,
        INotificationService notificationService
    )
    {
        _logger = logger;
        _dialogService = dialogService;
        _wpfDataService = wpfDataService;
        _mqttAppService = mqttAppService;
        _dataStorageService = dataStorageService;
        _mapper = mapper;
        _navigationService = navigationService;
        _notificationService = notificationService;

        
        _mqttServeise = _dataStorageService.MqttServers.ToNotifyCollectionChanged(x=>x.Value);
    }

    [RelayCommand]
    public async Task ToggleIsActive(MqttServerItemViewModel mqttServerItem)
    {
        try
        {
            if (mqttServerItem == null)
            {
                _notificationService.ShowError("没有选择任何MQTT服务器，请选择后再点击切换激活状态");
                return;
            }


            // 更新到数据存储
            await _wpfDataService.MqttDataService.UpdateMqttServer(mqttServerItem);

            // 显示操作结果
            var statusText = mqttServerItem.IsActive ? "已启用" : "已停用";
            _notificationService.ShowSuccess($"MQTT服务器 {mqttServerItem.ServerName} 已{statusText}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "切换MQTT服务器激活状态时发生错误");
            _notificationService.ShowError($"切换MQTT服务器激活状态时发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 添加MQTT服务器命令。
    /// </summary>
    [RelayCommand]
    public async Task AddMqtt()
    {
        try
        {
            // 1. 显示添加MQTT服务器对话框
            MqttServerItemViewModel mqtt = await _dialogService.ShowDialogAsync(new MqttDialogViewModel()
                                                                              {
                                                                                  Title = "添加MQTT服务器",
                                                                                  PrimaryButText = "添加MQTT服务器"
                                                                              });
            // 如果用户取消或对话框未返回MQTT服务器，则直接返回
            if (mqtt == null)
            {
                return;
            }

            var mqttItem = await _wpfDataService.MqttDataService.AddMqttServer(mqtt);
            _notificationService.ShowSuccess($"MQTT服务器添加成功：{mqttItem.ServerName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "添加MQTT服务器的过程中发生错误");
            _notificationService.ShowError($"添加MQTT服务器的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 删除MQTT服务器命令。
    /// </summary>
    [RelayCommand]
    public async Task DeleteMqtt()
    {
        try
        {
            if (SelectedMqtt == null)
            {
                _notificationService.ShowError("你没有选择任何MQTT服务器，请选择MQTT服务器后再点击删除");
                return;
            }

            var viewModel = new ConfirmDialogViewModel("删除MQTT服务器", $"确认要删除MQTT服务器名为:{SelectedMqtt.ServerName}", "删除MQTT服务器");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var mqttName = SelectedMqtt.ServerName;
                await _wpfDataService.MqttDataService.DeleteMqttServer(SelectedMqtt);
                _notificationService.ShowSuccess($"删除MQTT服务器成功,服务器名：{mqttName}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "删除MQTT服务器的过程中发生错误");
            _notificationService.ShowError($"删除MQTT服务器的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 编辑MQTT服务器命令。
    /// </summary>
    [RelayCommand]
    public async Task EditMqtt()
    {
        try
        {
            if (SelectedMqtt == null)
            {
                _notificationService.ShowError("你没有选择任何MQTT服务器，请选择MQTT服务器后再点击编辑");
                return;
            }

            MqttDialogViewModel mqttDialogViewModel = new MqttDialogViewModel(SelectedMqtt)
                                                          {
                                                              Title = "编辑MQTT服务器",
                                                              PrimaryButText = "保存修改"
                                                          };
            // 1. 显示MQTT服务器对话框
            MqttServerItemViewModel mqtt = await _dialogService.ShowDialogAsync(mqttDialogViewModel);
            // 如果用户取消或对话框未返回MQTT服务器，则直接返回
            if (mqtt == null)
            {
                return;
            }

            await _wpfDataService.MqttDataService.UpdateMqttServer(mqtt);
            
            // 更新UI
            _mapper.Map(mqtt, SelectedMqtt);
            
            _notificationService.ShowSuccess($"编辑MQTT服务器成功：{mqtt.ServerName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "编辑MQTT服务器的过程中发生错误");
            _notificationService.ShowError($"编辑MQTT服务器的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 导航到MQTT服务器详情页面。
    /// </summary>
    [RelayCommand]
    private async Task NavigateToMqttDetail()
    {
        if (SelectedMqtt == null) 
        {
            _notificationService.ShowWarn("请选择一个MQTT服务器以查看详情。");
            return;
        }

        // 导航到MQTT服务器详情页
        // var menu = new MenuItemViewModel
        // {
        //     TargetViewKey = "MqttServerDetailView",
        //     TargetId = SelectedMqtt.Id
        // };

        await _navigationService.NavigateToAsync(
            this, new NavigationParameter(nameof(MqttServerDetailViewModel), SelectedMqtt.Id, NavigationType.Mqtt));
    }
}