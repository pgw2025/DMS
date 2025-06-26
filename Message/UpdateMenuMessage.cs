using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PMSWPF.Message;

public class UpdateMenuMessage:ValueChangedMessage<int>
{
    public UpdateMenuMessage(int value) : base(value)
    {
    }
}