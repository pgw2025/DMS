using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Enums;
using PMSWPF.Message;

namespace PMSWPF.Helper;

public class NotificationHelper
{
    public static void ShowMessage(string msg, NotificationType notificationType=NotificationType.Info,bool isGlobal = false)
    {
        WeakReferenceMessenger.Default.Send<NotificationMessage>(
            new NotificationMessage(msg, notificationType));
    }
}