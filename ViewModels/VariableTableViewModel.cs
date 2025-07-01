using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;

namespace PMSWPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase
{
    [ObservableProperty]
    private VariableTable variableTable;
    [ObservableProperty]
    private ObservableCollection<DataVariable> _dataVariables;

    public override void OnLoaded()
    {
        if (VariableTable.DataVariables!=null )
        {
            _dataVariables = new ObservableCollection<DataVariable>(VariableTable.DataVariables);
        }
    }
}