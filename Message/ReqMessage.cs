using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PMSWPF.Message;

public class ReqMessage : RequestMessage<int>
{
    public ReqMessage(int count)
    {
        Count = count;
    }

    public int Count { get; set; }
}