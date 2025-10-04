using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Events
{
    /// <summary>
    /// MQTT服务器变更事件参数
    /// </summary>
    public class MqttServerChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public ActionChangeType ChangeType { get; }

        /// <summary>
        /// MQTT服务器DTO
        /// </summary>
        public MqttServerDto MqttServer { get; }
        
        /// <summary>
        /// 发生变化的属性类型
        /// </summary>
        public MqttServerPropertyType PropertyType { get; }
        

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="mqttServer">MQTT服务器DTO</param>
        /// <param name="propertyType">发生变化的属性类型</param>
        public MqttServerChangedEventArgs(ActionChangeType changeType, MqttServerDto mqttServer, MqttServerPropertyType propertyType = MqttServerPropertyType.All)
        {
            ChangeType = changeType;
            MqttServer = mqttServer;
            PropertyType = propertyType;
        }
    }
}