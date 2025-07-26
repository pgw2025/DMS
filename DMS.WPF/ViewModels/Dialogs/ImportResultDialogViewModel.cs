using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs;

/// <summary>
/// ImportResultDialogViewModel 是用于显示变量导入结果的视图模型。
/// 它包含成功导入和已存在变量的列表。
/// </summary>
public partial class ImportResultDialogViewModel : ObservableObject
{
    /// <summary>
    /// 成功导入的变量名称列表。
    /// </summary>
    public ObservableCollection<string> ImportedVariables { get; }

    /// <summary>
    /// 已存在的变量名称列表。
    /// </summary>
    public ObservableCollection<string> ExistingVariables { get; }

    /// <summary>
    /// 构造函数，初始化导入结果列表。
    /// </summary>
    /// <param name="importedVariables">成功导入的变量名称列表。</param>
    /// <param name="existingVariables">已存在的变量名称列表。</param>
    public ImportResultDialogViewModel(List<string> importedVariables, List<string> existingVariables)
    {
        ImportedVariables = new ObservableCollection<string>(importedVariables);
        ExistingVariables = new ObservableCollection<string>(existingVariables);
    }

    /// <summary>
    /// 关闭对话框的命令。
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        // 在实际应用中，这里可能需要通过IDialogService或其他机制来关闭对话框
        // 对于ContentDialog，通常不需要显式关闭命令，因为对话框本身有关闭按钮或通过Result返回
    }
}
