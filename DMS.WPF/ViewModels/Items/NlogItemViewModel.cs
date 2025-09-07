using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Models;

namespace DMS.WPF.ViewModels.Items;

public class NlogItemViewModel : ObservableObject
{
    private Nlog _nlog;

    public NlogItemViewModel()
    {
        _nlog = new Nlog();
    }

    public NlogItemViewModel(Nlog nlog)
    {
        _nlog = nlog;
    }

    public int Id => _nlog.Id;
    public string Level => _nlog.Level;
    public string ThreadName => _nlog.ThreadName;
    public string Callsite => _nlog.Callsite;
    public string Message => _nlog.Message;
    public string Logger => _nlog.Logger;
    public string Exception => _nlog.Exception;
    public string StackTrace => _nlog.Exception; // Using Exception as StackTrace since it's not in the Nlog model
    public System.DateTime TimeStamp => _nlog.LogTime;
}