using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VariableTableDialogViewModel:DialogViewModelBase<VariableTableItemViewModel>
{
    [ObservableProperty] 
    private VariableTableItemViewModel _variableTable;

    public VariableTableDialogViewModel(VariableTableItemViewModel variableTable=null)
    {
        if (variableTable==null)
        {
            VariableTable = new VariableTableItemViewModel();
        }
        else
        {
            VariableTable = variableTable;
        }
    }
    
    [RelayCommand]
    public async void ParimaryButton()
    {

       await Close(VariableTable);
    }
    [RelayCommand]
    public async void CancleButton()
    {

       await Close(null);
    }
}