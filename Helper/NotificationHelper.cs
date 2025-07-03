using CommunityToolkit.Mvvm.Messaging;
using NLog;
using PMSWPF.Enums;
using PMSWPF.Message;

namespace PMSWPF.Helper;

public class NotificationHelper
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    
    public static void ShowMessage(string msg, NotificationType notificationType = NotificationType.Info,
        bool isGlobal = false)
    {
        
        if (notificationType==NotificationType.Error)
        {
            Logger.Error(msg);
        }
        else
        {
            Logger.Info(msg);
        }
        WeakReferenceMessenger.Default.Send<NotificationMessage>(
            new NotificationMessage(msg, notificationType));
    }
}