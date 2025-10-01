using DMS.Core.Enums;

namespace DMS.Application.Events
{
    /// <summary>
    /// 设备状态改变事件参数
    /// 统一处理设备激活状态和连接状态的变更
    /// </summary>
    public class DeviceStateChangedEventArgs : System.EventArgs
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
        /// 状态值
        /// </summary>
        public bool StateValue { get; }
        
        /// <summary>
        /// 状态类型 (激活状态或连接状态)
        /// </summary>
        public DeviceStateType StateType { get; }
        
        /// <summary>
        /// 状态改变时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 初始化DeviceStateChangedEventArgs类的新实例
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="deviceName">设备名称</param>
        /// <param name="stateValue">状态值</param>
        /// <param name="stateType">状态类型</param>
        public DeviceStateChangedEventArgs(int deviceId, string deviceName, bool stateValue, DeviceStateType stateType)
        {
            DeviceId = deviceId;
            DeviceName = deviceName ?? string.Empty;
            StateValue = stateValue;
            StateType = stateType;
            ChangeTime = DateTime.Now;
        }
    }
}