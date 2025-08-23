using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VariableDialogViewModel : DialogViewModelBase<VariableItemViewModel>
{
    [ObservableProperty]
    private VariableItemViewModel _variable;

    public VariableDialogViewModel(string title,string primaryButText,VariableItemViewModel variable=null)
    {
        if (variable==null)
        {
            Variable=new VariableItemViewModel();
        }
        else
        {
            Variable=variable;
        }
        
        Title=title;
        PrimaryButText=primaryButText;
    }
    
    [RelayCommand]
    private void PrimaryButton()
    {
        Close(Variable);
    }
    [RelayCommand]
    private void CancleButton()
    {
        Close(null);
    }
}