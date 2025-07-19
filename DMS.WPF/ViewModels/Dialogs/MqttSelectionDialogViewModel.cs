using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Data.Repositories;
using System.Threading.Tasks;
using DMS.WPF.Models;
using DMS.Services;
using DMS.WPF.Services;
using DMS.WPF.Models;

namespace DMS.WPF.ViewModels.Dialogs;

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