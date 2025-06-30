using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;

namespace PMSWPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase
{
    [ObservableProperty]
    private VariableTable variableTable;
    public override void OnLoaded()
    {
    }
}