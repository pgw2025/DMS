using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.Logging;

namespace DMS.WPF.ViewModels;

/// <summary>
/// MQTT服务器管理视图模型，负责MQTT服务器的增删改查操作。
/// </summary>
public partial class MqttsViewModel : ViewModelBase
{
    private readonly DataServices _dataServices;
    private readonly IMqttAppService _mqttAppService;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// MQTT服务器列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MqttServerItemViewModel> _mqtts;

    /// <summary>
    /// 当前选中的MQTT服务器。
    /// </summary>
    [ObservableProperty]
    private MqttServerItemViewModel _selectedMqtt;

    private readonly ILogger<MqttsViewModel> _logger;

    public MqttsViewModel(
        ILogger<MqttsViewModel> logger, 
        IDialogService dialogService, 
        DataServices dataServices,
        IMqttAppService mqttAppService,
        IMapper mapper,
        INavigationService navigationService,
        INotificationService notificationService
    )
    {
        _logger = logger;
        _dialogService = dialogService;
        _dataServices = dataServices;
        _mqttAppService = mqttAppService;
        _mapper = mapper;
        _navigationService = navigationService;
        _notificationService = notificationService;
        
        Mqtts = _dataServices.MqttServers;
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

            var mqttItem = await _dataServices.AddMqttServer(_mqttAppService, mqtt);
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
                await _dataServices.DeleteMqttServer(_mqttAppService, SelectedMqtt);
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

            await _dataServices.UpdateMqttServer(_mqttAppService, mqtt);
            
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
        var menu = new MenuItemViewModel
        {
            TargetViewKey = "MqttServerDetailView",
            TargetId = SelectedMqtt.Id
        };
        
        await _navigationService.NavigateToAsync(menu);
    }
}