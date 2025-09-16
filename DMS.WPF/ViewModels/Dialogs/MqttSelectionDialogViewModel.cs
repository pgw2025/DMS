using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.ViewModels.Items;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DMS.Application.Interfaces.Database;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// MQTT服务器选择对话框的视图模型
    /// </summary>
    public partial class MqttSelectionDialogViewModel : DialogViewModelBase<MqttServerItemViewModel>
    {
        private readonly IMqttAppService _mqttAppService;

        [ObservableProperty]
        private ObservableCollection<MqttServerItemViewModel> _mqttServers = new();

        [ObservableProperty]
        private MqttServerItemViewModel _selectedMqttServer;

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
                var mqttServerDtos = await _mqttAppService.GetAllMqttServersAsync();
                MqttServers.Clear();

                foreach (var dto in mqttServerDtos)
                {
                    MqttServers.Add(new MqttServerItemViewModel
                    {
                        Id = dto.Id,
                        ServerName = dto.ServerName,
                        ServerUrl = dto.ServerUrl,
                        Port = dto.Port,
                        Username = dto.Username,
                        Password = dto.Password,
                        IsActive = dto.IsActive,
                        SubscribeTopic = dto.SubscribeTopic,
                        PublishTopic = dto.PublishTopic,
                        ClientId = dto.ClientId,
                        CreatedAt = dto.CreatedAt,
                        ConnectedAt = dto.ConnectedAt,
                        ConnectionDuration = dto.ConnectionDuration,
                        MessageFormat = dto.MessageFormat
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