using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DMS.Message;

public class UpdateMenuMessage:ValueChangedMessage<int>
{
    public UpdateMenuMessage(int value) : base(value)
    {
    }
}