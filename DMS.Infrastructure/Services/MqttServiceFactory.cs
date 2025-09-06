using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// MQTT服务工厂实现类，用于创建MQTT服务实例
    /// </summary>
    public class MqttServiceFactory : IMqttServiceFactory
    {
        private readonly ILogger<MqttService> _logger;

        /// <summary>
        /// 构造函数，注入日志记录器
        /// </summary>
        /// <param name="logger">MQTT服务日志记录器</param>
        public MqttServiceFactory(ILogger<MqttService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 创建MQTT服务实例
        /// </summary>
        /// <returns>IMqttService实例</returns>
        public IMqttService CreateService()
        {
            return new MqttService(_logger);
        }

        /// <summary>
        /// 根据MQTT服务器配置创建MQTT服务实例
        /// </summary>
        /// <param name="mqttServer">MQTT服务器配置</param>
        /// <returns>IMqttService实例</returns>
        public IMqttService CreateService(Core.Models.MqttServer mqttServer)
        {
            if (mqttServer == null)
                throw new ArgumentNullException(nameof(mqttServer));

            return new MqttService(_logger);
        }
    }
}