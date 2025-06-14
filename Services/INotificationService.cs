using PMSWPF.Enums;
using PMSWPF.Models;

namespace PMSWPF.Services;

public interface INotificationService
{
    void Show(Notification notification);
    void Show(string message, NotificationType type, bool IsGlobal);
}