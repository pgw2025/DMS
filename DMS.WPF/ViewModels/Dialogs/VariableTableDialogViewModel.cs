using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VariableTableDialogViewModel:DialogViewModelBase<VariableTableItem>
{
    [ObservableProperty] 
    private VariableTableItem _variableTable;

    public VariableTableDialogViewModel(VariableTableItem variableTable=null)
    {
        if (variableTable==null)
        {
            VariableTable = new VariableTableItem();
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