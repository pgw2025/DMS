using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Services.Mqtt
{
    /// <summary>
    /// MQTT服务管理器，负责管理MQTT连接和变量监控
    /// </summary>
    public class MqttServiceManager : IMqttServiceManager
    {
        private readonly ILogger<MqttServiceManager> _logger;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IAppCenterService _appDataCenterService;
        private readonly IMqttServiceFactory _mqttServiceFactory;
        private readonly IEventService _eventService;

        private readonly ConcurrentDictionary<int, MqttDeviceContext> _mqttContexts;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public MqttServiceManager(
            ILogger<MqttServiceManager> logger,
            IDataProcessingService dataProcessingService,
            IAppCenterService appDataCenterService,
            IMqttServiceFactory mqttServiceFactory,
            IEventService eventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _appDataCenterService = appDataCenterService ?? throw new ArgumentNullException(nameof(appDataCenterService));
            _mqttServiceFactory = mqttServiceFactory ?? throw new ArgumentNullException(nameof(mqttServiceFactory));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _mqttContexts = new ConcurrentDictionary<int, MqttDeviceContext>();
            _semaphore = new SemaphoreSlim(10, 10); // 默认最大并发连接数为10

            // 订阅MQTT服务器变更事件
            _eventService.OnMqttServerChanged += OnMqttServerChanged;
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
                MqttServerConfig = mqttServer,
                MqttService = _mqttServiceFactory.CreateService(),
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
        public void UpdateMqttAliases(int mqttServerId, List<MqttAlias> mqttAliases)
        {
            if (_mqttContexts.TryGetValue(mqttServerId, out var context))
            {
                context.MqttAliases.Clear();
                foreach (var alias in mqttAliases)
                {
                    context.MqttAliases.AddOrUpdate(alias.Id, alias, (key, oldValue) => alias);
                }
                _logger.LogInformation("已更新MQTT服务器 {MqttServerId} 的变量别名列表，共 {Count} 个别名", mqttServerId, mqttAliases.Count);
            }
        }

        /// <summary>
        /// 获取MQTT服务器连接状态
        /// </summary>
        public bool IsMqttServerConnected(int mqttServerId)
        {
            return _mqttContexts.TryGetValue(mqttServerId, out var context) && context.MqttService.IsConnected;
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
                    context.MqttServerConfig.ServerName, context.MqttServerConfig.ServerUrl, context.MqttServerConfig.Port);

                var stopwatch = Stopwatch.StartNew();

                // 设置连接超时
                using var timeoutToken = new CancellationTokenSource(5000); // 5秒超时
                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);

                await context.MqttService.ConnectAsync(
                    context.MqttServerConfig.ServerUrl,
                    context.MqttServerConfig.Port,
                    context.MqttServerConfig.ClientId,
                    context.MqttServerConfig.Username,
                    context.MqttServerConfig.Password);

                stopwatch.Stop();
                _logger.LogInformation("MQTT服务器 {ServerName} 连接耗时 {ElapsedMs} ms",
                    context.MqttServerConfig.ServerName, stopwatch.ElapsedMilliseconds);
                if (context.MqttService.IsConnected)
                {
                    context.ReconnectAttempts = 0; // 重置重连次数
                    context.MqttServerConfig.IsConnect=true;
                    // 订阅主题
                    if (!string.IsNullOrEmpty(context.MqttServerConfig.SubscribeTopic))
                    {
                        await context.MqttService.SubscribeAsync(context.MqttServerConfig.SubscribeTopic);
                    }

                    _logger.LogInformation("MQTT服务器 {ServerName} 连接成功", context.MqttServerConfig.ServerName);

                    //
                }
                else
                {
                    context.MqttServerConfig.IsConnect = false;
                    _logger.LogWarning("MQTT服务器 {ServerName} 连接失败", context.MqttServerConfig.ServerName);
                }
                _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Updated, context.MqttServerConfig, MqttServerPropertyType.IsConnect));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接MQTT服务器 {ServerName} 时发生错误: {ErrorMessage}",
                    context.MqttServerConfig.ServerName, ex.Message);
                context.ReconnectAttempts++;
                context.MqttServerConfig.IsConnect = false;
                _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Updated, context.MqttServerConfig, MqttServerPropertyType.IsConnect));
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
                _logger.LogInformation("正在断开MQTT服务器 {ServerName} 的连接", context.MqttServerConfig.ServerName);
                await context.MqttService.DisconnectAsync();
                _logger.LogInformation("MQTT服务器 {ServerName} 连接已断开", context.MqttServerConfig.ServerName);

                // 如果连接状态从连接变为断开，触发事件
                context.MqttServerConfig.IsConnect = false;
                _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Updated, context.MqttServerConfig, MqttServerPropertyType.IsConnect));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开MQTT服务器 {ServerName} 连接时发生错误: {ErrorMessage}",
                    context.MqttServerConfig.ServerName, ex.Message);
            }
        }

        /// <summary>
        /// 发布变量数据到MQTT服务器
        /// </summary>
        public async Task PublishVariableDataAsync(MqttAlias variableMqtt, CancellationToken cancellationToken = default)
        {
            if (variableMqtt?.MqttServer == null || variableMqtt.Variable == null)
            {
                _logger.LogWarning("无效的VariableMqtt对象，跳过发布");
                return;
            }

            if (!_mqttContexts.TryGetValue(variableMqtt.MqttServer.Id, out var context))
            {
                _logger.LogWarning("未找到MQTT服务器 {MqttServerId}", variableMqtt.MqttServer.Id);
                return;
            }

            if (!context.MqttService.IsConnected)
            {
                _logger.LogWarning("MQTT服务器 {ServerName} 未连接，跳过发布", context.MqttServerConfig.ServerName);
                return;
            }

            try
            {
                var topic = context.MqttServerConfig.PublishTopic;

                var sendMsg = BuildSendMessage(variableMqtt);

                await context.MqttService.PublishAsync(topic, sendMsg);
                _logger.LogDebug("成功向MQTT服务器 {ServerName} 发布变量 {VariableName} 的数据：{sendMsg}",
                    context.MqttServerConfig.ServerName, variableMqtt.Variable.Name, sendMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "向MQTT服务器 {ServerName} 发布变量 {VariableName} 数据时发生错误: {ErrorMessage}",
                    context.MqttServerConfig.ServerName, variableMqtt.Variable.Name, ex.Message);
            }
        }

        private string BuildSendMessage(MqttAlias variableMqtt)
        {
            StringBuilder sb = new StringBuilder();
            var now = DateTime.Now;
            var timestamp = ((DateTimeOffset)now).ToUnixTimeMilliseconds();
            if (!String.IsNullOrWhiteSpace(variableMqtt.MqttServer.MessageHeader))
                sb.Append(variableMqtt.MqttServer.MessageHeader.Replace("{timestamp}", timestamp.ToString()));
            if (!String.IsNullOrWhiteSpace(variableMqtt.MqttServer.MessageContent))
                sb.Append(variableMqtt.MqttServer.MessageContent.Replace("{name}", variableMqtt.Alias).Replace("{value}", variableMqtt.Variable.DisplayValue));
            if (!String.IsNullOrWhiteSpace(variableMqtt.MqttServer.MessageFooter))
                sb.Append(variableMqtt.MqttServer.MessageFooter);

            return sb.ToString();
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

                if (!context.MqttService.IsConnected)
                {
                    _logger.LogWarning("MQTT服务器 {ServerName} 未连接，跳过 {Count} 条消息",
                        context.MqttServerConfig.ServerName, group.Count());
                    continue;
                }

                try
                {
                    foreach (var variableMqtt in group)
                    {
                        var topic = context.MqttServerConfig.PublishTopic;
                        var payload = variableMqtt.Variable?.DataValue?.ToString() ?? string.Empty;

                        await context.MqttService.PublishAsync(topic, payload);
                    }

                    _logger.LogInformation("成功向MQTT服务器 {ServerName} 发布 {Count} 条变量数据",
                        context.MqttServerConfig.ServerName, group.Count());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "向MQTT服务器 {ServerName} 批量发布变量数据时发生错误: {ErrorMessage}",
                        context.MqttServerConfig.ServerName, ex.Message);
                }
            }
        }

        /// <summary>
        /// 处理MQTT服务器变更事件
        /// </summary>
        private async void OnMqttServerChanged(object? sender, MqttServerChangedEventArgs e)
        {
            try
            {
                
                _logger.LogDebug("处理MQTT服务器变更事件: 服务器ID={MqttServerId}, 变更类型={ChangeType}, 变更属性={PropertyType}",
                    e.MqttServer.Id, e.ChangeType, e.PropertyType);

                switch (e.ChangeType)
                {
                    case ActionChangeType.Added:
                        HandleMqttServerAdded(e.MqttServer);
                        break;
                    case ActionChangeType.Updated:
                        await HandleMqttServerUpdated(e.MqttServer, e.PropertyType);
                        break;
                    case ActionChangeType.Deleted:
                        await HandleMqttServerRemoved(e.MqttServer.Id);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理MQTT服务器变更事件时发生错误: 服务器ID={MqttServerId}, 变更类型={ChangeType}",
                    e.MqttServer.Id, e.ChangeType);
            }
        }

        /// <summary>
        /// 处理MQTT服务器添加事件
        /// </summary>
        private void HandleMqttServerAdded(MqttServer mqttServer)
        {
            if (mqttServer == null)
            {
                _logger.LogWarning("HandleMqttServerAdded: 接收到空MQTT服务器对象");
                return;
            }

            try
            {
                _logger.LogInformation("处理MQTT服务器添加事件: {MqttServerId} ({MqttServerName})",
                    mqttServer.Id, mqttServer.ServerName);

                // 添加服务器到监控列表
                AddMqttServer(mqttServer);

                // 如果服务器是激活状态，则尝试连接
                if (mqttServer.IsActive)
                {
                    // 使用Task.Run来避免阻塞事件处理
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ConnectMqttServerAsync(mqttServer.Id, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "连接新添加的MQTT服务器 {MqttServerId} ({MqttServerName}) 时发生错误",
                                mqttServer.Id, mqttServer.ServerName);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理MQTT服务器添加事件时发生错误: {MqttServerId} ({MqttServerName})",
                    mqttServer?.Id, mqttServer?.ServerName);
            }
        }

        /// <summary>
        /// 处理MQTT服务器更新事件
        /// </summary>
        private async Task HandleMqttServerUpdated(MqttServer mqttServer, MqttServerPropertyType propertyType)
        {
            if (mqttServer == null)
            {
                _logger.LogWarning("HandleMqttServerUpdated: 接收到空MQTT服务器对象");
                return;
            }

            try
            {
                _logger.LogInformation("处理MQTT服务器更新事件: {MqttServerId} ({MqttServerName}), 属性类型: {PropertyType}",
                    mqttServer.Id, mqttServer.ServerName, propertyType);

                // 检查上下文是否存在
                if (_mqttContexts.TryGetValue(mqttServer.Id, out var context))
                {
                    // 根据属性类型决定是否需要重新连接或特殊处理
                    switch (propertyType)
                    {
                        case MqttServerPropertyType.ServerUrl:
                        case MqttServerPropertyType.Port:
                        case MqttServerPropertyType.Username:
                        case MqttServerPropertyType.Password:
                        case MqttServerPropertyType.ClientId:
                            // 这些属性修改后需要重新连接
                            // 更新配置
                            context.MqttServerConfig.ServerUrl = mqttServer.ServerUrl;
                            context.MqttServerConfig.Port = mqttServer.Port;
                            context.MqttServerConfig.Username = mqttServer.Username;
                            context.MqttServerConfig.Password = mqttServer.Password;
                            context.MqttServerConfig.ClientId = mqttServer.ClientId;
                            
                            // 重新连接服务器
                            await ReconnectMqttServerAsync(mqttServer.Id, CancellationToken.None);
                            break;
                        case MqttServerPropertyType.IsActive:
                                
                                if (mqttServer.IsActive)
                                {
                                    // 激活状态变为true，连接服务器
                                    await ConnectMqttServerAsync(mqttServer.Id, CancellationToken.None);
                                }
                                else
                                {
                                    // 激活状态变为false，断开服务器连接
                                    await DisconnectMqttServerAsync(mqttServer.Id, CancellationToken.None);
                                }
                            break;
                        case MqttServerPropertyType.SubscribeTopic:
                            context.MqttServerConfig.SubscribeTopic = mqttServer.SubscribeTopic;
                            // 更新订阅主题
                            if (context.MqttService.IsConnected && !string.IsNullOrEmpty(mqttServer.SubscribeTopic))
                            {
                                await context.MqttService.SubscribeAsync(mqttServer.SubscribeTopic);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理MQTT服务器更新事件时发生错误: {MqttServerId} ({MqttServerName})",
                    mqttServer?.Id, mqttServer?.ServerName);
            }
        }

        /// <summary>
        /// 处理MQTT服务器删除事件
        /// </summary>
        private async Task HandleMqttServerRemoved(int mqttServerId)
        {
            try
            {
                _logger.LogInformation("处理MQTT服务器删除事件: {MqttServerId}", mqttServerId);

                // 从监控列表中移除服务器
                await RemoveMqttServerAsync(mqttServerId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理MQTT服务器删除事件时发生错误: {MqttServerId}", mqttServerId);
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
                
                // 取消事件订阅
                _eventService.OnMqttServerChanged -= OnMqttServerChanged;

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
                            context.MqttServerConfig?.ServerName ?? "Unknown");
                    }
                }

                _logger.LogInformation("MQTT服务管理器已释放资源");
            }
        }
    }
}