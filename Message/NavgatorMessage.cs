using CommunityToolkit.Mvvm.Messaging.Messages;
using PMSWPF.ViewModels;

namespace PMSWPF.Message;

public class NavgatorMessage : ValueChangedMessage<ViewModelBase>
{
    public Object Parameters;
    public NavgatorMessage(ViewModelBase value,Object parameters=null) : base(value)
    {
        Parameters=parameters;
    }
}