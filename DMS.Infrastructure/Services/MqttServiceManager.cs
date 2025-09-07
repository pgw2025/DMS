using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces.Services;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// MQTT服务管理器，负责管理MQTT连接和变量监控
    /// </summary>
    public class MqttServiceManager : IMqttServiceManager
    {
        private readonly ILogger<MqttServiceManager> _logger;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IDataCenterService _dataCenterService;
        private readonly IMqttServiceFactory _mqttServiceFactory;
        private readonly ConcurrentDictionary<int, MqttDeviceContext> _mqttContexts;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public MqttServiceManager(
            ILogger<MqttServiceManager> logger,
            IDataProcessingService dataProcessingService,
            IDataCenterService dataCenterService,
            IMqttServiceFactory mqttServiceFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _dataCenterService = dataCenterService ?? throw new ArgumentNullException(nameof(dataCenterService));
            _mqttServiceFactory = mqttServiceFactory ?? throw new ArgumentNullException(nameof(mqttServiceFactory));
            _mqttContexts = new ConcurrentDictionary<int, MqttDeviceContext>();
            _semaphore = new SemaphoreSlim(10, 10); // 默认最大并发连接数为10
        }

        /// <summary>
        /// 初始化服务管理器
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MQTT服务管理器正在初始化...");
            // 初始化逻辑可以在需要时添加
            _logger.LogInformation("MQTT服务管理器初始化完成");
        }

        /// <summary>
        /// 添加MQTT服务器到监控列表
        /// </summary>
        public void AddMqttServer(MqttServer mqttServer)
        {
            if (mqttServer == null)
                throw new ArgumentNullException(nameof(mqttServer));

            var context = new MqttDeviceContext
            {
                MqttServer = mqttServer,
                MqttService = _mqttServiceFactory.CreateService(),
                IsConnected = false
            };

            _mqttContexts.AddOrUpdate(mqttServer.Id, context, (key, oldValue) => context);
            _logger.LogInformation("已添加MQTT服务器 {MqttServerId} 到监控列表", mqttServer.Id);
        }

        /// <summary>
        /// 移除MQTT服务器监控
        /// </summary>
        public async Task RemoveMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default)
        {
            if (_mqttContexts.TryRemove(mqttServerId, out var context))
            {
                await DisconnectMqttServerAsync(mqttServerId, cancellationToken);
                _logger.LogInformation("已移除MQTT服务器 {MqttServerId} 的监控", mqttServerId);
            }
        }

        /// <summary>
        /// 更新MQTT服务器变量别名
        /// </summary>
        public void UpdateVariableMqttAliases(int mqttServerId, List<VariableMqttAlias> variableMqttAliases)
        {
            if (_mqttContexts.TryGetValue(mqttServerId, out var context))
            {
                context.VariableMqttAliases.Clear();
                foreach (var alias in variableMqttAliases)
                {
                    context.VariableMqttAliases.AddOrUpdate(alias.Id, alias, (key, oldValue) => alias);
                }
                _logger.LogInformation("已更新MQTT服务器 {MqttServerId} 的变量别名列表，共 {Count} 个别名", mqttServerId, variableMqttAliases.Count);
            }
        }

        /// <summary>
        /// 获取MQTT服务器连接状态
        /// </summary>
        public bool IsMqttServerConnected(int mqttServerId)
        {
            return _mqttContexts.TryGetValue(mqttServerId, out var context) && context.IsConnected;
        }

        /// <summary>
        /// 重新连接MQTT服务器
        /// </summary>
        public async Task ReconnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default)
        {
            if (_mqttContexts.TryGetValue(mqttServerId, out var context))
            {
                await DisconnectMqttServerAsync(mqttServerId, cancellationToken);
                await ConnectMqttServerAsync(mqttServerId, cancellationToken);
            }
        }

        /// <summary>
        /// 获取所有监控的MQTT服务器ID
        /// </summary>
        public IEnumerable<int> GetMonitoredMqttServerIds()
        {
            return _mqttContexts.Keys.ToList();
        }

        /// <summary>
        /// 连接MQTT服务器
        /// </summary>
        public async Task ConnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default)
        {
            if (!_mqttContexts.TryGetValue(mqttServerId, out var context))
            {
                _logger.LogWarning("未找到MQTT服务器 {MqttServerId}", mqttServerId);
                return;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("正在连接MQTT服务器 {ServerName} ({ServerUrl}:{Port})",
                    context.MqttServer.ServerName, context.MqttServer.ServerUrl, context.MqttServer.Port);

                var stopwatch = Stopwatch.StartNew();

                // 设置连接超时
                using var timeoutToken = new CancellationTokenSource(5000); // 5秒超时
                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);

                await context.MqttService.ConnectAsync(
                    context.MqttServer.ServerUrl,
                    context.MqttServer.Port,
                    context.MqttServer.ClientId,
                    context.MqttServer.Username,
                    context.MqttServer.Password);

                stopwatch.Stop();
                _logger.LogInformation("MQTT服务器 {ServerName} 连接耗时 {ElapsedMs} ms",
                    context.MqttServer.ServerName, stopwatch.ElapsedMilliseconds);

                if (context.MqttService.IsConnected)
                {
                    context.IsConnected = true;
                    context.ReconnectAttempts = 0; // 重置重连次数
                    
                    // 订阅主题
                    if (!string.IsNullOrEmpty(context.MqttServer.SubscribeTopic))
                    {
                        await context.MqttService.SubscribeAsync(context.MqttServer.SubscribeTopic);
                    }
                    
                    _logger.LogInformation("MQTT服务器 {ServerName} 连接成功", context.MqttServer.ServerName);
                }
                else
                {
                    _logger.LogWarning("MQTT服务器 {ServerName} 连接失败", context.MqttServer.ServerName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接MQTT服务器 {ServerName} 时发生错误: {ErrorMessage}",
                    context.MqttServer.ServerName, ex.Message);
                context.IsConnected = false;
                context.ReconnectAttempts++;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 断开MQTT服务器连接
        /// </summary>
        public async Task DisconnectMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default)
        {
            if (!_mqttContexts.TryGetValue(mqttServerId, out var context))
                return;

            try
            {
                _logger.LogInformation("正在断开MQTT服务器 {ServerName} 的连接", context.MqttServer.ServerName);
                await context.MqttService.DisconnectAsync();
                context.IsConnected = false;
                _logger.LogInformation("MQTT服务器 {ServerName} 连接已断开", context.MqttServer.ServerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开MQTT服务器 {ServerName} 连接时发生错误: {ErrorMessage}",
                    context.MqttServer.ServerName, ex.Message);
            }
        }

        /// <summary>
        /// 发布变量数据到MQTT服务器
        /// </summary>
        public async Task PublishVariableDataAsync(VariableMqtt variableMqtt, CancellationToken cancellationToken = default)
        {
            if (variableMqtt?.Mqtt == null || variableMqtt.Variable == null)
            {
                _logger.LogWarning("无效的VariableMqtt对象，跳过发布");
                return;
            }

            if (!_mqttContexts.TryGetValue(variableMqtt.Mqtt.Id, out var context))
            {
                _logger.LogWarning("未找到MQTT服务器 {MqttServerId}", variableMqtt.Mqtt.Id);
                return;
            }

            if (!context.IsConnected)
            {
                _logger.LogWarning("MQTT服务器 {ServerName} 未连接，跳过发布", context.MqttServer.ServerName);
                return;
            }

            try
            {
                var topic = context.MqttServer.PublishTopic;
                var payload = variableMqtt.Variable.DataValue?.ToString() ?? string.Empty;

                await context.MqttService.PublishAsync(topic, payload);
                _logger.LogDebug("成功向MQTT服务器 {ServerName} 发布变量 {VariableName} 的数据",
                    context.MqttServer.ServerName, variableMqtt.Variable.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "向MQTT服务器 {ServerName} 发布变量 {VariableName} 数据时发生错误: {ErrorMessage}",
                    context.MqttServer.ServerName, variableMqtt.Variable.Name, ex.Message);
            }
        }

        /// <summary>
        /// 发布批量变量数据到MQTT服务器
        /// </summary>
        public async Task PublishVariablesDataAsync(List<VariableMqtt> variableMqtts, CancellationToken cancellationToken = default)
        {
            if (variableMqtts == null || !variableMqtts.Any())
            {
                _logger.LogWarning("变量MQTT列表为空，跳过批量发布");
                return;
            }

            // 按MQTT服务器ID进行分组
            var groupedByMqtt = variableMqtts.GroupBy(vm => vm.Mqtt.Id);

            foreach (var group in groupedByMqtt)
            {
                var mqttId = group.Key;
                if (!_mqttContexts.TryGetValue(mqttId, out var context))
                {
                    _logger.LogWarning("未找到MQTT服务器 {MqttServerId}，跳过 {Count} 条消息", mqttId, group.Count());
                    continue;
                }

                if (!context.IsConnected)
                {
                    _logger.LogWarning("MQTT服务器 {ServerName} 未连接，跳过 {Count} 条消息", 
                        context.MqttServer.ServerName, group.Count());
                    continue;
                }

                try
                {
                    foreach (var variableMqtt in group)
                    {
                        var topic = context.MqttServer.PublishTopic;
                        var payload = variableMqtt.Variable?.DataValue?.ToString() ?? string.Empty;

                        await context.MqttService.PublishAsync(topic, payload);
                    }
                    
                    _logger.LogInformation("成功向MQTT服务器 {ServerName} 发布 {Count} 条变量数据",
                        context.MqttServer.ServerName, group.Count());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "向MQTT服务器 {ServerName} 批量发布变量数据时发生错误: {ErrorMessage}",
                        context.MqttServer.ServerName, ex.Message);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _semaphore?.Dispose();
                
                // 断开所有MQTT连接
                foreach (var context in _mqttContexts.Values)
                {
                    try
                    {
                        context.MqttService?.DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "断开MQTT服务器 {ServerName} 连接时发生错误",
                            context.MqttServer?.ServerName ?? "Unknown");
                    }
                }
                
                _logger.LogInformation("MQTT服务管理器已释放资源");
            }
        }
    }
}