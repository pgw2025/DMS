using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Events
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
        /// 设备DTO
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="device">设备DTO</param>
        public DeviceChangedEventArgs(DataChangeType changeType, Device device)
        {
            ChangeType = changeType;
            Device = device;
            ChangeTime = DateTime.Now;
        }
    }
}