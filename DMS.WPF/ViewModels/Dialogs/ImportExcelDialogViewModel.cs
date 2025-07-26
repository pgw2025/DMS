using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ImportExcelDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private ObservableCollection<DMS.Core.Models.Variable> _variables = new();

    partial void OnFilePathChanged(string? value)
    {
        // if (string.IsNullOrEmpty(value))
        // {
        //     return;
        // }
        //
        // try
        // {
        //     var data = ExcelHelper.ImprotFromTiaVariableTable(value);
        //     Variables = new ObservableCollection<DMS.Core.Models.Variable>(data);
        // }
        // catch (System.Exception ex)
        // {
        //     // Handle exception
        // }
    }
}