using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;
using PMSWPF.Data.Repositories;
using System.Threading.Tasks;
using PMSWPF.Services;

namespace PMSWPF.ViewModels.Dialogs;

public partial class MqttSelectionDialogViewModel : ObservableObject
{

    [ObservableProperty]
    private ObservableCollection<Mqtt> mqtts;

    [ObservableProperty]
    private Mqtt? selectedMqtt;

    public MqttSelectionDialogViewModel(DataServices dataServices)
    {
        Mqtts = new ObservableCollection<Mqtt>(dataServices.Mqtts);
    }


}