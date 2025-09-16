namespace DMS.Core.Events;

public class DeviceConnectChangedEventArgs
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public int DeviceId { get; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; }

    /// <summary>
    /// 新状态
    /// </summary>
    public bool NewStatus { get; }

    /// <summary>
    /// 初始化DeviceStatusChangedEventArgs类的新实例
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="deviceName">设备名称</param>
    /// <param name="oldStatus">旧状态</param>
    /// <param name="newStatus">新状态</param>
    public DeviceConnectChangedEventArgs(int deviceId, string deviceName, bool newStatus)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        NewStatus = newStatus;
    }
}