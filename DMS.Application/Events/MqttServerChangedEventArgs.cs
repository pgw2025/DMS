using DMS.Core.Enums;
using DMS.Core.Models;

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
        /// MQTT服务器
        /// </summary>
        public MqttServer MqttServer { get; }
        
        /// <summary>
        /// 发生变化的属性类型
        /// </summary>
        public MqttServerPropertyType PropertyType { get; }
        

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="mqttServer">MQTT服务器</param>
        /// <param name="propertyType">发生变化的属性类型</param>
        public MqttServerChangedEventArgs(ActionChangeType changeType, MqttServer mqttServer, MqttServerPropertyType propertyType = MqttServerPropertyType.All)
        {
            ChangeType = changeType;
            MqttServer = mqttServer;
            PropertyType = propertyType;
        }
    }
}