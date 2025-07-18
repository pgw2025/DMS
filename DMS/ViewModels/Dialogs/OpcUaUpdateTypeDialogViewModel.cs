using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Enums;

namespace DMS.ViewModels.Dialogs
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