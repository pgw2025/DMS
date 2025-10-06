using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ItemViewModel;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace DMS.WPF.ViewModels
{
    /// <summary>
    /// MQTT服务器详情视图模型。
    /// 负责管理单个MQTT服务器的配置及其关联的变量数据。
    /// </summary>
    public partial class MqttServerDetailViewModel : ViewModelBase
    {
        private readonly ILogger<MqttServerDetailViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IEventService _eventService;
        private readonly IMqttManagementService _mqttManagementService;
        private readonly IWPFDataService _wpfDataService;
        private readonly IDataStorageService _dataStorageService;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// 当前正在编辑的MQTT服务器对象。
        /// </summary>
        [ObservableProperty]
        private MqttServerItem _currentMqtt;

        /// <summary>
        /// 与当前MQTT服务器关联的变量数据集合。
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MqttAlias> _associatedVariables;


        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志服务。</param>
        /// <param name="dataServices">数据服务。</param>
        /// <param name="dialogService">对话框服务。</param>
        /// <param name="notificationService">通知服务。</param>
        /// <param name="mqttManagementService">MQTT管理服务</param>
        /// <param name="navigationService">导航服务</param>
        public MqttServerDetailViewModel(ILogger<MqttServerDetailViewModel> logger,
                                         IDialogService dialogService,
                                         INotificationService notificationService,
                                         IEventService eventService,
                                         IMqttManagementService mqttManagementService,
                                         IWPFDataService wpfDataService,
                                         IDataStorageService dataStorageService,
                                         INavigationService navigationService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _notificationService = notificationService;
            this._eventService = eventService;
            _mqttManagementService = mqttManagementService;
            this._wpfDataService = wpfDataService;
            this._dataStorageService = dataStorageService;
            _navigationService = navigationService;
        }

      

        /// <summary>
        /// 编辑当前MQTT服务器
        /// </summary>
        [RelayCommand]
        private async Task EditMqtt()
        {
            try
            {
                if (CurrentMqtt == null)
                {
                    _notificationService.ShowError("没有选中的MQTT服务器，无法编辑。");
                    return;
                }

                // 创建编辑对话框的视图模型
                var mqttDialogViewModel = new MqttDialogViewModel(CurrentMqtt)
                {
                    Title = "编辑MQTT服务器",
                    PrimaryButText = "保存修改"
                };

                // 显示对话框
                var updatedMqtt = await _dialogService.ShowDialogAsync(mqttDialogViewModel);

                if (updatedMqtt == null)
                {
                    return; // 用户取消了编辑
                }

                // 更新MQTT服务器
                var result = await _wpfDataService.MqttDataService.UpdateMqttServer(updatedMqtt);
                
                if (result)
                {
                    // 更新当前视图模型的数据
                    CurrentMqtt.ServerName = updatedMqtt.ServerName;
                    CurrentMqtt.ServerUrl = updatedMqtt.ServerUrl;
                    CurrentMqtt.Port = updatedMqtt.Port;
                    CurrentMqtt.ClientId = updatedMqtt.ClientId;
                    CurrentMqtt.Username = updatedMqtt.Username;
                    CurrentMqtt.Password = updatedMqtt.Password;
                    CurrentMqtt.PublishTopic = updatedMqtt.PublishTopic;
                    CurrentMqtt.SubscribeTopic = updatedMqtt.SubscribeTopic;
                    CurrentMqtt.MessageHeader = updatedMqtt.MessageHeader;
                    CurrentMqtt.MessageContent = updatedMqtt.MessageContent;
                    CurrentMqtt.MessageFooter = updatedMqtt.MessageFooter;

                    _notificationService.ShowSuccess($"MQTT服务器编辑成功：{updatedMqtt.ServerName}");
                }
                else
                {
                    _notificationService.ShowError("更新MQTT服务器失败。");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "编辑MQTT服务器过程中发生错误");
                _notificationService.ShowError($"编辑MQTT服务器过程中发生错误：{e.Message}", e);
            }
        }
        
        /// <summary>
        /// 重新加载当前MQTT服务器数据
        /// </summary>
        [RelayCommand]
        private async Task Reload()
        {
            if (CurrentMqtt?.Id > 0)
            {
                // 重新加载当前MQTT服务器数据
                var updatedMqtt = await _mqttManagementService.GetMqttServerByIdAsync(CurrentMqtt.Id);
                if (updatedMqtt != null)
                {
                    // 更新CurrentMqtt的属性
                    CurrentMqtt.ServerName = updatedMqtt.ServerName;
                    CurrentMqtt.ServerUrl = updatedMqtt.ServerUrl;
                    CurrentMqtt.Port = updatedMqtt.Port;
                    CurrentMqtt.ClientId = updatedMqtt.ClientId;
                    CurrentMqtt.Username = updatedMqtt.Username;
                    CurrentMqtt.Password = updatedMqtt.Password;
                    CurrentMqtt.PublishTopic = updatedMqtt.PublishTopic;
                    CurrentMqtt.SubscribeTopic = updatedMqtt.SubscribeTopic;
                    CurrentMqtt.MessageHeader = updatedMqtt.MessageHeader;
                    CurrentMqtt.MessageContent = updatedMqtt.MessageContent;
                    CurrentMqtt.MessageFooter = updatedMqtt.MessageFooter;
                }
            }
        }

        /// <summary>
        /// 导航回MQTT服务器列表页面
        /// </summary>
        [RelayCommand]
        private async Task NavigateToMqtts()
        {
            await _navigationService.NavigateToAsync(this, new NavigationParameter(nameof(MqttsViewModel)));
        }

        public override Task OnNavigatedToAsync(NavigationParameter parameter)
        {
            if (parameter == null) return Task.CompletedTask;

            if (_dataStorageService.MqttServers.TryGetValue(parameter.TargetId, out var mqttServerItem))
            {
                CurrentMqtt = mqttServerItem;

            }
            _eventService.OnVariableValueChanged += OnVariableValueChanged;

            return Task.CompletedTask;
        }

        private void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e)
        {

            var variableAlias=CurrentMqtt.VariableAliases.FirstOrDefault(v => v.Variable.Id == e.Variable.Id);
            if (variableAlias is not null)
            {
                variableAlias.Variable.DisplayValue=e.Variable.DisplayValue;
                variableAlias.Variable.UpdatedAt=e.Variable.UpdatedAt;

            }
        }

        /// <summary>
        /// 修改变量的MQTT发送名称
        /// </summary>
        [RelayCommand]
        private async Task ModifyAlias(MqttAlias variableAlias)
        {
            if (variableAlias == null)
            {
                _notificationService.ShowError("请选择要修改的变量项。");
                return;
            }

            try
            {
                // 创建一个用于输入新名称的简单对话框
                var oldAlias = variableAlias.Alias;
                InputDialogViewModel viewModel = new InputDialogViewModel("修改发送名称", "请输入新的MQTT发送名称:", oldAlias);
                var dialogResult = await _dialogService.ShowDialogAsync(viewModel);
                
                if (dialogResult != null) // 用户没有取消操作
                {
                    var newAlias = dialogResult.Trim();
                    
                    if (string.IsNullOrEmpty(newAlias))
                    {
                        _notificationService.ShowWarn("发送名称不能为空。");
                        return;
                    }

                    // 更新变量的发送名称
                    variableAlias.Alias = newAlias;

                    // 保存更改到数据服务
                    // var result = await _wpfDataService.UpdateMqttServer(CurrentMqtt);
                    //
                    // if (result)
                    // {
                    //     _notificationService.ShowSuccess($"变量 '{variableAlias.Variable.Name}' 的发送名称已更新为 '{newAlias}'");
                    // }
                    // else
                    // {
                    //     _notificationService.ShowError("更新发送名称失败。");
                    //     // 如果更新失败，恢复原来的值
                    //     variableAlias.Alias = oldAlias;
                    // }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "修改变量发送名称时发生错误");
                _notificationService.ShowError($"修改发送名称时发生错误：{e.Message}");
            }
        }
        
        public override Task OnNavigatedFromAsync(NavigationParameter parameter)
        {

            _eventService.OnVariableValueChanged -= OnVariableValueChanged;

            return Task.CompletedTask;
        }
    }
}