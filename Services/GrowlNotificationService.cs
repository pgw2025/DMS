using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using PMSWPF.Enums;
using PMSWPF.Message;
using Notification = PMSWPF.Models.Notification;

namespace PMSWPF.Services;

public class GrowlNotificationService :ObservableRecipient,IRecipient<NotificationMessage>
{
    public GrowlNotificationService()
    {
        IsActive = true;
    }
    public void Show(Notification notification)
    {
        if (notification == null )
        {
            return;
        }

        if (notification.IsGlobal)
        {
            switch (notification.Type)
            {
                case NotificationType.Info:
                    Growl.InfoGlobal(notification.Message);
                    break;
                case NotificationType.Error:
                    Growl.ErrorGlobal(notification.Message);
                    break;
                case NotificationType.Warning:
                    Growl.WarningGlobal(notification.Message);
                    break;
                case NotificationType.Success:
                    Growl.SuccessGlobal(notification.Message);
                    break;
                case NotificationType.Fatal:
                    Growl.FatalGlobal(notification.Message);
                    break;
                case NotificationType.Clear:
                    Growl.ClearGlobal();
                    break;
                default:
                    Growl.InfoGlobal(notification.Message);
                    break;
            }

        }
        else
        {
            switch (notification.Type)
            {
                case NotificationType.Info:
                    Growl.Info(notification.Message);
                    break;
                case NotificationType.Error:
                    Growl.Error(notification.Message);
                    break;
                case NotificationType.Warning:
                    Growl.Warning(notification.Message);
                    break;
                case NotificationType.Success:
                    Growl.Success(notification.Message);
                    break;
                case NotificationType.Fatal:
                    Growl.Fatal(notification.Message);
                    break;
                case NotificationType.Clear:
                    Growl.Clear();
                    break;
                default:
                    Growl.Info(notification.Message);
                    break;
            }
        }
    }

    public void Show(string message, NotificationType type=NotificationType.Info,bool IsGlobal=true)
    {
        Show(new Notification(){Message = message,Type = type,IsGlobal = IsGlobal});
    }

    public void Receive(NotificationMessage message)
    {
        Show(message.Value,message.Type,message.IsGlobal);
    }
}