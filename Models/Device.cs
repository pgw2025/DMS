using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.Models;

public partial class Device:ObservableObject
{
    [ObservableProperty]
    private int id;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string description ;
    [ObservableProperty]
    private string ip ;
    [ObservableProperty]
    private bool isActive ;
    [ObservableProperty]
    private bool isRuning ;
    
}