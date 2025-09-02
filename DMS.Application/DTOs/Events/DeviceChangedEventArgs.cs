using System;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 设备变更事件参数
    /// </summary>
    public class DeviceChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="deviceName">设备名称</param>
        public DeviceChangedEventArgs(DataChangeType changeType, int deviceId, string deviceName)
        {
            ChangeType = changeType;
            DeviceId = deviceId;
            DeviceName = deviceName;
            ChangeTime = DateTime.Now;
        }
    }
}