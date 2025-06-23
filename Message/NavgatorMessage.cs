using CommunityToolkit.Mvvm.Messaging.Messages;
using PMSWPF.ViewModels;

namespace PMSWPF.Message;

public class NavgatorMessage : ValueChangedMessage<ViewModelBase>
{
    public NavgatorMessage(ViewModelBase value) : base(value)
    {
    }
}