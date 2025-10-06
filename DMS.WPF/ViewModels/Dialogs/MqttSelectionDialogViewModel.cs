using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DMS.Application.Interfaces;
using DMS.WPF.ItemViewModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DMS.Application.Interfaces.Database;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// MQTT服务器选择对话框的视图模型
    /// </summary>
    public partial class MqttSelectionDialogViewModel : DialogViewModelBase<MqttServerItem>
    {
        private readonly IMqttAppService _mqttAppService;

        [ObservableProperty]
        private ObservableCollection<MqttServerItem> _mqttServers = new();

        [ObservableProperty]
        private MqttServerItem _selectedMqttServer;

        public MqttSelectionDialogViewModel(IMqttAppService mqttAppService)
        {
            _mqttAppService = mqttAppService;
            LoadMqttServersAsync();
        }

        /// <summary>
        /// 异步加载所有MQTT服务器
        /// </summary>
        private async void LoadMqttServersAsync()
        {
            try
            {
                var mqttServers = await _mqttAppService.GetAllMqttServersAsync();
                MqttServers.Clear();

                foreach (var mqttServer in mqttServers)
                {
                    MqttServers.Add(new MqttServerItem
                    {
                        Id = mqttServer.Id,
                        ServerName = mqttServer.ServerName,
                        ServerUrl = mqttServer.ServerUrl,
                        Port = mqttServer.Port,
                        Username = mqttServer.Username,
                        Password = mqttServer.Password,
                        IsActive = mqttServer.IsActive,
                        SubscribeTopic = mqttServer.SubscribeTopic,
                        PublishTopic = mqttServer.PublishTopic,
                        ClientId = mqttServer.ClientId,
                        CreatedAt = mqttServer.CreatedAt,
                        ConnectedAt = mqttServer.ConnectedAt,
                        ConnectionDuration = mqttServer.ConnectionDuration,
                        MessageFormat = mqttServer.MessageFormat
                    });
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志
                System.Console.WriteLine($"加载MQTT服务器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            if (SelectedMqttServer != null)
            {
                Close(SelectedMqttServer);
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Close(null);
        }
    }
}