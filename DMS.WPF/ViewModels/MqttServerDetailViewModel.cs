using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces.Management;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;
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
        private readonly IMqttManagementService _mqttManagementService;
        private readonly IDataStorageService _dataStorageService;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// 当前正在编辑的MQTT服务器对象。
        /// </summary>
        [ObservableProperty]
        private MqttServerItemViewModel _currentMqtt;

        /// <summary>
        /// 与当前MQTT服务器关联的变量数据集合。
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<VariableMqttAlias> _associatedVariables;


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
                                         IMqttManagementService mqttManagementService,
                                         IDataStorageService dataStorageService,
                                         INavigationService navigationService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _notificationService = notificationService;
            _mqttManagementService = mqttManagementService;
            this._dataStorageService = dataStorageService;
            _navigationService = navigationService;
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


            return Task.CompletedTask;
        }
    }
}