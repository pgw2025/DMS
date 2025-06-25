using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PMSWPF.Message;

public class OpenDialogMessage:RequestMessage<DialogMessage>
{
    // public bool IsCancel { get; set; }
    // public bool IsConfirm { get; set; }
    // public Object? Request { get; set; }
    // public Object? Response { get; set; }
    public DialogMessage Message { get; set; }

    public OpenDialogMessage()
    {
    }

    public OpenDialogMessage(DialogMessage message)
    {
        Message = message;
    }
}