using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PMSWPF.Message;

public class MyMessage : ValueChangedMessage<int>
{
    public MyMessage(int value) : base(value)
    {
    }

    public int Count { get; set; }
}