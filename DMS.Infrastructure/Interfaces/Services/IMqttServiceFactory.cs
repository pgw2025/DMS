using DMS.Core.Models;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// MQTT服务工厂接口，用于创建MQTT服务实例
    /// </summary>
    public interface IMqttServiceFactory
    {
        /// <summary>
        /// 创建MQTT服务实例
        /// </summary>
        /// <returns>IMqttService实例</returns>
        IMqttService CreateService();

        /// <summary>
        /// 根据MQTT服务器配置创建MQTT服务实例
        /// </summary>
        /// <param name="mqttServer">MQTT服务器配置</param>
        /// <returns>IMqttService实例</returns>
        IMqttService CreateService(MqttServer mqttServer);
    }
}