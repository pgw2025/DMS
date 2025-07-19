using CommunityToolkit.Mvvm.Messaging.Messages;
using DMS.WPF.ViewModels;

namespace DMS.Message;

public class NavgatorMessage : ValueChangedMessage<ViewModelBase>
{
    public Object Parameters;
    public NavgatorMessage(ViewModelBase value,Object parameters=null) : base(value)
    {
        Parameters=parameters;
    }
}