using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class ImportExcelDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private ObservableCollection<VariableData> _variableData = new();

    partial void OnFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        try
        {
            var data = ExcelHelper.ImprotFromTiaVariableTable(value);
            VariableData = new ObservableCollection<VariableData>(data);
        }
        catch (System.Exception ex)
        {
            // Handle exception
        }
    }
}