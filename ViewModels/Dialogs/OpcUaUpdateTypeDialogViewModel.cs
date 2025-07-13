using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Enums;

namespace PMSWPF.ViewModels.Dialogs
{
    public partial class OpcUaUpdateTypeDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private OpcUaUpdateType _selectedUpdateType;
       

        public OpcUaUpdateTypeDialogViewModel()
        {
            // 默认选中第一个
            SelectedUpdateType = OpcUaUpdateType.OpcUaPoll;
        }
    }
}