using DMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// MQTT服务管理器接口，负责管理MQTT连接和变量监控
    /// </summary>
    public interface IMqttServiceManager : IDisposable
    {
        /// <summary>
        /// 初始化服务管理器
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 添加MQTT服务器到监控列表
        /// </summary>
        void AddMqttServer(MqttServer mqttServer);

        /// <summary>
        /// 移除MQTT服务器监控
        /// </summary>
        Task RemoveMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新MQTT服务器变量别名
        /// </summary>
        void UpdateVariableMqttAliases(int mqttServerId, List<VariableMqttAlias> variableMqttAliases);

        /// <summary>
        /// 获取MQTT服务器连接状态
        /// </summary>
        bool IsMqttServerConnected(int mqttServerId);

        /// <summary>
        /// 重新连接MQTT服务器
        /// </summary>
        Task ReconnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有监控的MQTT服务器ID
        /// </summary>
        IEnumerable<int> GetMonitoredMqttServerIds();

        /// <summary>
        /// 连接MQTT服务器
        /// </summary>
        Task ConnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 断开MQTT服务器连接
        /// </summary>
        Task DisconnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default);

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