using DMS.Enums;
using DMS.Models;

namespace DMS.Services;

public interface INotificationService
{
    void Show(Notification notification);
    void Show(string message, NotificationType type = NotificationType.Info, bool IsGlobal = true);
}