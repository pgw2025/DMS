using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Data.Entities;
using System.Collections.ObjectModel;

namespace PMSWPF.ViewModels
{
    public partial class PlcListViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private ObservableCollection<PLC> plcList;
        public PlcListViewModel()
        {
            plcList = new ObservableCollection<PLC>();
        
        }
    }
}
