using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PMSWPF.Data;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.Views;

namespace PMSWPF.ViewModels;

public partial class MqttsViewModel : ViewModelBase
{
    private readonly DataServices _dataServices;
    private readonly IDialogService _dialogService;
    private readonly MqttRepository _mqttRepository;
    private readonly ILogger<MqttsViewModel> _logger;
    private readonly NavgatorServices _navgatorServices;

    private ObservableCollection<Mqtt> _mqtts;

    public ObservableCollection<Mqtt> Mqtts
    {
        get => _mqtts;
        set
        {
            if (_mqtts != null)
            {
                foreach (var mqtt in _mqtts)
                {
                    mqtt.PropertyChanged -= Mqtt_PropertyChanged;
                }
            }

            SetProperty(ref _mqtts, value);

            if (_mqtts != null)
            {
                foreach (var mqtt in _mqtts)
                {
                    mqtt.PropertyChanged += Mqtt_PropertyChanged;
                }
            }
        }
    }

    [ObservableProperty]
    private Mqtt _selectedMqtt;

    public MqttsViewModel(
        ILogger<MqttsViewModel> logger, IDialogService dialogService, DataServices dataServices, NavgatorServices navgatorServices
    )
    {
        _mqttRepository = new MqttRepository();
        _logger = logger;
        _dialogService = dialogService;
        _dataServices = dataServices;
        _navgatorServices = navgatorServices;

        if (dataServices.Mqtts == null || dataServices.Mqtts.Count == 0)
        {
            MessageHelper.SendLoadMessage(LoadTypes.Mqtts);
        }
        else
        {
            Mqtts = new ObservableCollection<Mqtt>(dataServices.Mqtts);
        }

        
        _dataServices.OnMqttListChanged += (sender, mqtts) =>
        {
            Mqtts = new ObservableCollection<Mqtt>(mqtts);
        };
    }

    private async void Mqtt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Mqtt.IsActive))
        {
            if (sender is Mqtt mqtt)
            {
                try
                {
                    await _mqttRepository.Edit(mqtt);
                    NotificationHelper.ShowSuccess($"MQTT: {mqtt.Name} 的启用状态已更新。");
                    MessageHelper.SendLoadMessage(LoadTypes.Mqtts);
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"更新MQTT启用状态失败: {mqtt.Name} - {ex.Message}", ex);
                }
            }
        }
    }

    [RelayCommand]
    public async void AddMqtt()
    {
        try
        {
            var mqtt = await _dialogService.ShowAddMqttDialog();
            if (mqtt == null)
            {
                _logger.LogInformation("用户取消了添加MQTT操作。");
                return;
            }

            await _mqttRepository.Add(mqtt);
            MessageHelper.SendLoadMessage(LoadTypes.Mqtts);
            MessageHelper.SendLoadMessage(LoadTypes.Menu);
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"添加MQTT的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public async void DeleteMqtt()
    {
        try
        {
            if (SelectedMqtt == null)
            {
                NotificationHelper.ShowError("你没有选择任何MQTT，请选择MQTT后再点击删除");
                return;
            }

            string msg = $"确认要删除MQTT名为:{SelectedMqtt.Name}";
            var isDel = await _dialogService.ShowConfrimeDialog("删除MQTT", msg, "删除MQTT");
            if (isDel)
            {
                await _mqttRepository.Delete(SelectedMqtt);
                MessageHelper.SendLoadMessage(LoadTypes.Mqtts);
                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                NotificationHelper.ShowSuccess($"删除MQTT成功,MQTT名：{SelectedMqtt.Name}");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除MQTT的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public async void EditMqtt()
    {
        try
        {
            if (SelectedMqtt == null)
            {
                NotificationHelper.ShowError("你没有选择任何MQTT，请选择MQTT后再点击编辑");
                return;
            }

            var editMqtt = await _dialogService.ShowEditMqttDialog(SelectedMqtt);
            if (editMqtt != null)
            {
                var res = await _mqttRepository.Edit(editMqtt);
                MessageHelper.SendLoadMessage(LoadTypes.Mqtts);
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"编辑MQTT的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 导航到MQTT服务器详情页面。
    /// </summary>
    [RelayCommand]
    private void NavigateToMqttDetail()
    {
        // if (SelectedMqtt == null)
        // {
        //     NotificationHelper.ShowMessage("请选择一个MQTT服务器以查看详情。", NotificationType.Warning);
        //     return;
        // }
        // _navgatorServices.NavigateTo<MqttServerDetailView>(SelectedMqtt);
    }
}