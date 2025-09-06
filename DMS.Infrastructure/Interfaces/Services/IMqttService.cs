using DMS.Core.Models;
using MQTTnet.Client;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// MQTT服务接口，定义MQTT客户端的基本操作
    /// </summary>
    public interface IMqttService
    {
        /// <summary>
        /// 获取MQTT客户端连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步连接到MQTT服务器
        /// </summary>
        /// <param name="serverUrl">MQTT服务器URL</param>
        /// <param name="port">端口号</param>
        /// <param name="clientId">客户端ID</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        Task ConnectAsync(string serverUrl, int port, string clientId, string username, string password);

        /// <summary>
        /// 异步断开MQTT连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 异步发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">消息内容</param>
        Task PublishAsync(string topic, string payload);

        /// <summary>
        /// 异步订阅主题
        /// </summary>
        /// <param name="topic">要订阅的主题</param>
        Task SubscribeAsync(string topic);

        /// <summary>
        /// 设置消息接收处理程序
        /// </summary>
        /// <param name="handler">消息处理回调函数</param>
        void SetMessageHandler(Func<string, string, Task> handler);
    }
}