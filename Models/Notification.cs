using PMSWPF.Enums;

namespace PMSWPF.Models;

public class Notification
{
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsGlobal { get; set; }
}