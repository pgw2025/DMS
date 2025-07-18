using CommunityToolkit.Mvvm.Messaging.Messages;
using DMS.Core.Enums;

namespace DMS.Message;

public class LoadMessage:ValueChangedMessage<LoadTypes>
{
    public LoadTypes LoadType { get; set; }
    public LoadMessage(LoadTypes types) : base(types)
    {
        LoadType=types;
    }
}