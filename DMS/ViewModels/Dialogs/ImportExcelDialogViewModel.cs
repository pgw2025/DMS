using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Helper;
using DMS.Models;

namespace DMS.ViewModels.Dialogs;

public partial class ImportExcelDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private ObservableCollection<Variable> _variables = new();

    partial void OnFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        try
        {
            var data = ExcelHelper.ImprotFromTiaVariableTable(value);
            Variables = new ObservableCollection<Variable>(data);
        }
        catch (System.Exception ex)
        {
            // Handle exception
        }
    }
}