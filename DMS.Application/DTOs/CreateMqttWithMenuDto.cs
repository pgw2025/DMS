using DMS.Application.DTOs;

namespace DMS.Application.DTOs
{
    /// <summary>
    /// 创建MQTT服务器及其关联菜单的数据传输对象
    /// </summary>
    public class CreateMqttWithMenuDto
    {
        /// <summary>
        /// MQTT服务器信息
        /// </summary>
        public MqttServerDto MqttServer { get; set; }
        
        /// <summary>
        /// 菜单项信息
        /// </summary>
        public MenuBeanDto MqttServerMenu { get; set; }
    }
}