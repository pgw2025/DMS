// 文件: DMS.WPF/ViewModels/Items/NlogItemViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表日志列表中的单个日志项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// </summary>
public partial class NlogItemViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private DateTime _logTime;

    [ObservableProperty]
    private string _level;

    [ObservableProperty]
    private int _threadId;

    [ObservableProperty]
    private string _threadName;

    [ObservableProperty]
    private string _callsite;

    [ObservableProperty]
    private int _callsiteLineNumber;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private string _logger;

    [ObservableProperty]
    private string _exception;

    [ObservableProperty]
    private string _callerFilePath;

    [ObservableProperty]
    private int _callerLineNumber;

    [ObservableProperty]
    private string _callerMember;
}