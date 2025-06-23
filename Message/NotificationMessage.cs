using CommunityToolkit.Mvvm.Messaging.Messages;
using PMSWPF.Enums;

namespace PMSWPF.Message;

public class NotificationMessage:ValueChangedMessage<string>
{
    public NotificationType Type { get; set; }
    public bool IsGlobal { get; set; }
    public NotificationMessage(string msg,NotificationType type=NotificationType.Info,bool isGlobal=false) : base(msg)
    {
        this.Type = type;
        this.IsGlobal = isGlobal;
    }
}