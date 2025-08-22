using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Helper;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ImportExcelDialogViewModel : DialogViewModelBase<List<Variable>>
{
    private readonly IExcelService _excelService;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private List<Variable> _variables = new();

    [ObservableProperty]
    private IList _selectedVariables = new ArrayList();

    public ImportExcelDialogViewModel(IExcelService excelService)
    {
        _excelService = excelService;
    }

    partial void OnFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        
        try
        {
           Variables = _excelService.ImprotFromTiaVariableTable(value);
        }
        catch (System.Exception ex)
        {
            NotificationHelper.ShowError($"从Excel文件中读取变量时发生了错误:{ex.Message}",ex);
        }
    }
    
    [RelayCommand]
    private void ImportAll()
    {
         Close(Variables);
    }

    [RelayCommand]
    private void ImportSelected()
    {
        var selected = SelectedVariables.Cast<Variable>().ToList();
        Close(selected);
    }

    [RelayCommand]
    private void CancleButton()
    {
        Close(null);
    }
}
