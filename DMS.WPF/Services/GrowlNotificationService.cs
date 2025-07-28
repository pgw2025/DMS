using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Core.Enums;
using DMS.Message;
using HandyControl.Controls;

namespace DMS.WPF.Services;

public class GrowlNotificationService : ObservableRecipient, IRecipient<NotificationMessage>
{
    public GrowlNotificationService()
    {
        IsActive = true;
    }

    public void Receive(NotificationMessage message)
    {
        Show(message.Value, message.Type, message.IsGlobal);
    }
    
    public void Show(string message, NotificationType type = NotificationType.Info, bool IsGlobal = true)
    {
    
        if (IsGlobal)
            switch (type)
            {
                case NotificationType.Info:
                    Growl.InfoGlobal(message);
                    break;
                case NotificationType.Error:
                    Growl.ErrorGlobal(message);
                    break;
                case NotificationType.Warning:
                    Growl.WarningGlobal(message);
                    break;
                case NotificationType.Success:
                    Growl.SuccessGlobal(message);
                    break;
                case NotificationType.Fatal:
                    Growl.FatalGlobal(message);
                    break;
                case NotificationType.Clear:
                    Growl.ClearGlobal();
                    break;
                default:
                    Growl.InfoGlobal(message);
                    break;
            }
        else
            switch (type)
            {
                case NotificationType.Info:
                    Growl.Info(message);
                    break;
                case NotificationType.Error:
                    Growl.Error(message);
                    break;
                case NotificationType.Warning:
                    Growl.Warning(message);
                    break;
                case NotificationType.Success:
                    Growl.Success(message);
                    break;
                case NotificationType.Fatal:
                    Growl.Fatal(message);
                    break;
                case NotificationType.Clear:
                    Growl.Clear();
                    break;
                default:
                    Growl.Info(message);
                    break;
            }
    }

}