using CommunityToolkit.Mvvm.Messaging.Messages;
using PMSWPF.Enums;

namespace PMSWPF.Message;

public class LoadMessage:ValueChangedMessage<LoadTypes>
{
    public LoadMessage(LoadTypes types) : base(types)
    {
    }
}