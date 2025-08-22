using System.Collections.ObjectModel;
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
    private readonly IMapper _mapper;
    private readonly IExcelService _excelService;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private List<Variable> _variables = new();
    


    public ImportExcelDialogViewModel(IMapper mapper,IExcelService excelService)
    {
        _mapper = mapper;
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
    public void SecondaryButton()
    {
       
    }
    
    [RelayCommand]
    private void PrimaryButton()
    {
         Close(Variables);
    }
    [RelayCommand]
    private void CancleButton()
    {
        Close(null);
    }
}