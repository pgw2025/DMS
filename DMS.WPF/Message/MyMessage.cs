using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DMS.Message;

public class MyMessage : ValueChangedMessage<int>
{
    public MyMessage(int value) : base(value)
    {
    }

    public int Count { get; set; }
}