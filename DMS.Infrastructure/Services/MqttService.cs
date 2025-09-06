using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// MQTT服务实现类，用于与MQTT服务器进行通信
    /// </summary>
    public class MqttService : IMqttService
    {
        private IMqttClient _mqttClient;
        private readonly ILogger<MqttService> _logger;
        private readonly IMqttClientOptions _options;
        private Func<string, string, Task> _messageHandler;
        private string _clientId;

        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        /// <summary>
        /// 构造函数，注入日志记录器
        /// </summary>
        /// <param name="logger">日志记录器实例</param>
        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _clientId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 异步连接到MQTT服务器
        /// </summary>
        public async Task ConnectAsync(string serverUrl, int port, string clientId, string username, string password)
        {
            try
            {
                _clientId = clientId ?? Guid.NewGuid().ToString();
                
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(serverUrl, port)
                    .WithClientId(_clientId)
                    .WithCredentials(username, password)
                    .WithCleanSession()
                    .Build();

                _mqttClient.UseConnectedHandler(async e => await HandleConnectedAsync(e));
                _mqttClient.UseDisconnectedHandler(async e => await HandleDisconnectedAsync(e));
                _mqttClient.UseApplicationMessageReceivedHandler(async e => await HandleMessageReceivedAsync(e));

                await _mqttClient.ConnectAsync(options);
                _logger.LogInformation($"成功连接到MQTT服务器: {serverUrl}:{port} (ClientID: {_clientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"连接MQTT服务器时发生错误: {ex.Message} (ClientID: {_clientId})");
                throw;
            }
        }

        /// <summary>
        /// 异步断开MQTT连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_mqttClient != null && _mqttClient.IsConnected)
                {
                    await _mqttClient.DisconnectAsync();
                    _logger.LogInformation($"已断开MQTT服务器连接 (ClientID: {_clientId})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"断开MQTT服务器连接时发生错误: {ex.Message} (ClientID: {_clientId})");
                throw;
            }
        }

        /// <summary>
        /// 异步发布消息
        /// </summary>
        public async Task PublishAsync(string topic, string payload)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("MQTT客户端未连接");
            }

            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload ?? string.Empty)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
                _logger.LogDebug($"成功发布消息到主题 {topic} (ClientID: {_clientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发布消息到主题 {topic} 时发生错误: {ex.Message} (ClientID: {_clientId})");
                throw;
            }
        }

        /// <summary>
        /// 异步订阅主题
        /// </summary>
        public async Task SubscribeAsync(string topic)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("MQTT客户端未连接");
            }

            try
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
                _logger.LogInformation($"成功订阅主题: {topic} (ClientID: {_clientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"订阅主题 {topic} 时发生错误: {ex.Message} (ClientID: {_clientId})");
                throw;
            }
        }

        /// <summary>
        /// 设置消息接收处理程序
        /// </summary>
        public void SetMessageHandler(Func<string, string, Task> handler)
        {
            _messageHandler = handler;
        }

        #region 私有处理方法

        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation($"MQTT客户端连接成功 (ClientID: {_clientId})");
        }

        private async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning($"MQTT客户端断开连接: {args.Reason} (ClientID: {_clientId})");
        }

        private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            
            _logger.LogDebug($"收到MQTT消息 - 主题: {topic}, 内容: {payload} (ClientID: {_clientId})");

            if (_messageHandler != null)
            {
                await _messageHandler(topic, payload);
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_mqttClient != null && _mqttClient.IsConnected)
                {
                    _mqttClient.DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
                }
                _mqttClient?.Dispose();
                _logger.LogInformation($"MQTT服务资源已释放 (ClientID: {_clientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"释放MQTT服务资源时发生错误: {ex.Message} (ClientID: {_clientId})");
            }
        }
    }
}