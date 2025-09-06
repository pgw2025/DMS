using System;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// MQTT服务器变更事件参数
    /// </summary>
    public class MqttServerChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// MQTT服务器DTO
        /// </summary>
        public MqttServerDto MqttServer { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="mqttServer">MQTT服务器DTO</param>
        public MqttServerChangedEventArgs(DataChangeType changeType, MqttServerDto mqttServer)
        {
            ChangeType = changeType;
            MqttServer = mqttServer;
            ChangeTime = DateTime.Now;
        }
    }
}