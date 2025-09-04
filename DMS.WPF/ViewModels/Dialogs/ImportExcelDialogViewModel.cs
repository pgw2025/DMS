using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ImportExcelDialogViewModel : DialogViewModelBase<List<Variable>>
{
    private readonly IMapper _mapper;
    private readonly IExcelService _excelService;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private List<Variable> _variables = new();
    
    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _variableItemViewModels ;

    [ObservableProperty]
    private IList _selectedVariables = new ArrayList();

    public ImportExcelDialogViewModel(IMapper mapper,IExcelService excelService, NotificationService notificationService)
    {
        _mapper = mapper;
        _excelService = excelService;
        _notificationService = notificationService;
        VariableItemViewModels = new();
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
           VariableItemViewModels=new ObservableCollection<VariableItemViewModel>(_mapper.Map<List<VariableItemViewModel>>(Variables));
        }
        catch (System.Exception ex)
        {
            _notificationService.ShowError($"从Excel文件中读取变量时发生了错误:{ex.Message}",ex);
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
        var selected = SelectedVariables.Cast<VariableItemViewModel>().ToList();
        Close(_mapper.Map<List<Variable>>(selected));
    }

    [RelayCommand]
    private void CancleButton()
    {
        Close(null);
    }
}
