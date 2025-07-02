using CommunityToolkit.Mvvm.Messaging.Messages;
using PMSWPF.Enums;

namespace PMSWPF.Message;

public class LoadMessage:ValueChangedMessage<LoadTypes>
{
    public LoadTypes LoadType { get; set; }
    public LoadMessage(LoadTypes types) : base(types)
    {
        LoadType=types;
    }
}