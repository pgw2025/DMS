using System;

namespace DMS.WPF.Events;

/// <summary>
/// 设备状态改变事件参数
/// </summary>
public class DeviceActiveChangedEventArgs : EventArgs
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
    /// 旧状态
    /// </summary>
    public bool OldStatus { get; }

    /// <summary>
    /// 新状态
    /// </summary>
    public bool NewStatus { get; }

    /// <summary>
    /// 状态改变时间
    /// </summary>
    public DateTime ChangeTime { get; }

    /// <summary>
    /// 初始化DeviceStatusChangedEventArgs类的新实例
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="deviceName">设备名称</param>
    /// <param name="oldStatus">旧状态</param>
    /// <param name="newStatus">新状态</param>
    public DeviceActiveChangedEventArgs(int deviceId, string deviceName, bool oldStatus, bool newStatus)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangeTime = DateTime.Now;
    }
}