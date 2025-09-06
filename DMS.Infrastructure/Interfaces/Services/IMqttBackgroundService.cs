using DMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// MQTT后台服务接口，负责管理MQTT连接和数据传输
    /// </summary>
    public interface IMqttBackgroundService : IDisposable
    {
        /// <summary>
        /// 启动MQTT后台服务
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止MQTT后台服务
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 添加MQTT服务器配置
        /// </summary>
        void AddMqttServer(MqttServer mqttServer);

        /// <summary>
        /// 移除MQTT服务器配置
        /// </summary>
        Task RemoveMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新MQTT服务器配置
        /// </summary>
        void UpdateMqttServer(MqttServer mqttServer);

        /// <summary>
        /// 获取所有MQTT服务器配置
        /// </summary>
        IEnumerable<MqttServer> GetAllMqttServers();

        /// <summary>
        /// 发布变量数据到MQTT服务器
        /// </summary>
        Task PublishVariableDataAsync(VariableMqtt variableMqtt, CancellationToken cancellationToken = default);

        /// <summary>
        /// 发布批量变量数据到MQTT服务器
        /// </summary>
        Task PublishVariablesDataAsync(List<VariableMqtt> variableMqtts, CancellationToken cancellationToken = default);
    }
}