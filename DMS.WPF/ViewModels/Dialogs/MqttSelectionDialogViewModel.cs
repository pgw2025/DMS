using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Data.Repositories;
using System.Threading.Tasks;
using DMS.Models;
using DMS.Services;

namespace DMS.ViewModels.Dialogs;

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