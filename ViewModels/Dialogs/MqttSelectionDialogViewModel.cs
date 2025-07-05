using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;
using PMSWPF.Data.Repositories;
using System.Threading.Tasks;

namespace PMSWPF.ViewModels.Dialogs;

public partial class MqttSelectionDialogViewModel : ObservableObject
{
    private readonly MqttRepository _mqttRepository;

    [ObservableProperty]
    private ObservableCollection<Mqtt> mqtts;

    [ObservableProperty]
    private Mqtt? selectedMqtt;

    public MqttSelectionDialogViewModel()
    {
        _mqttRepository = new MqttRepository();
        LoadMqtts();
    }

    private async void LoadMqtts()
    {
        try
        {
            var allMqtts = await _mqttRepository.GetAll();
            Mqtts = new ObservableCollection<Mqtt>(allMqtts);
        }
        catch (Exception ex)
        {
            // 这里需要一个日志记录器，但由于ViewModel中没有直接注入ILogger，
            // 暂时使用Console.WriteLine或NotificationHelper
            // 更好的做法是注入ILogger或使用静态日志类
            Console.WriteLine($"加载MQTT服务器列表失败: {ex.Message}");
            // 或者使用NotificationHelper.ShowMessage("加载MQTT服务器列表失败", NotificationType.Error);
        }
    }
}