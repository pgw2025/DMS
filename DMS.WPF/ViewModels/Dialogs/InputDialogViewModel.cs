using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class InputDialogViewModel : DialogViewModelBase<string?>
{
    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _inputLabel = "输入";

    [ObservableProperty]
    private string _inputPlaceholder = "请输入...";

    [ObservableProperty]
    private string? _inputText;

    [ObservableProperty]
    private string _primaryButtonText = "确定";

    [ObservableProperty]
    private string _closeButtonText = "取消";

    public InputDialogViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public InputDialogViewModel(string title, string message, string inputLabel) : this(title, message)
    {
        InputLabel = inputLabel;
    }

    public InputDialogViewModel(string title, string message, string inputLabel, string inputPlaceholder) : this(title, message, inputLabel)
    {
        InputPlaceholder = inputPlaceholder;
    }

    public InputDialogViewModel(string title, string message, string inputLabel, string inputPlaceholder, string? defaultValue) : this(title, message, inputLabel, inputPlaceholder)
    {
        InputText = defaultValue;
    }

    public InputDialogViewModel()
    {
        Title = "输入";
        Message = "请输入信息：";
    }

    [RelayCommand]
    private void Confirm()
    {
        Close(InputText);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close(null);
    }
}