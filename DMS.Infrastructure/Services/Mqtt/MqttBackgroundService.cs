using System.Collections.Concurrent;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Services.Mqtt
{
    /// <summary>
    /// MQTT后台服务，负责管理MQTT连接和数据传输
    /// </summary>
    public class MqttBackgroundService : BackgroundService, IMqttBackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IMqttServiceManager _mqttServiceManager;
        private readonly IEventService _eventService;
        private readonly IAppStorageService _appStorageService;
        private readonly IAppCenterService _appCenterService;
        private readonly ConcurrentDictionary<int, MqttServer> _mqttServers;
        private readonly SemaphoreSlim _reloadSemaphore = new(0);

        public MqttBackgroundService(
            ILogger<MqttBackgroundService> logger,
            IMqttServiceManager mqttServiceManager,
            IEventService eventService,
            IAppStorageService appStorageService,
            IAppCenterService appCenterService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mqttServiceManager = mqttServiceManager ?? throw new ArgumentNullException(nameof(mqttServiceManager));
            _eventService = eventService;
            _appStorageService = appStorageService;
            _appCenterService = appCenterService ?? throw new ArgumentNullException(nameof(appCenterService));
            _mqttServers = new ConcurrentDictionary<int, MqttServer>();

            _eventService.OnLoadDataCompleted += OnLoadDataCompleted;
        }

        private void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Start();
            }

        }

        /// <summary>
        /// 启动MQTT后台服务
        /// </summary>
        private void Start(CancellationToken cancellationToken = default)
        {
            _reloadSemaphore.Release();
            _logger.LogInformation("MQTT后台服务启动请求已发送");
        }

        /// <summary>
        /// 停止MQTT后台服务
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MQTT后台服务停止请求已发送");
        }

        /// <summary>
        /// 添加MQTT服务器配置
        /// </summary>
        public void AddMqttServer(MqttServer mqttServer)
        {
            if (mqttServer == null)
                throw new ArgumentNullException(nameof(mqttServer));

            _mqttServers.AddOrUpdate(mqttServer.Id, mqttServer, (key, oldValue) => mqttServer);
            _mqttServiceManager.AddMqttServer(mqttServer);
            _reloadSemaphore.Release();
            _logger.LogInformation("已添加MQTT服务器 {ServerName} 到监控列表", mqttServer.ServerName);
        }

        /// <summary>
        /// 移除MQTT服务器配置
        /// </summary>
        public async Task RemoveMqttServerAsync(int mqttServerId, CancellationToken cancellationToken = default)
        {
            if (_mqttServers.TryRemove(mqttServerId, out var mqttServer))
            {
                await _mqttServiceManager.RemoveMqttServerAsync(mqttServerId, cancellationToken);
                _logger.LogInformation("已移除MQTT服务器 {ServerName} 的监控", mqttServer?.ServerName ?? mqttServerId.ToString());
            }
        }

        /// <summary>
        /// 更新MQTT服务器配置
        /// </summary>
        public void UpdateMqttServer(MqttServer mqttServer)
        {
            if (mqttServer == null)
                throw new ArgumentNullException(nameof(mqttServer));

            _mqttServers.AddOrUpdate(mqttServer.Id, mqttServer, (key, oldValue) => mqttServer);
            _reloadSemaphore.Release();
            _logger.LogInformation("已更新MQTT服务器 {ServerName} 的配置", mqttServer.ServerName);
        }

        /// <summary>
        /// 获取所有MQTT服务器配置
        /// </summary>
        public IEnumerable<MqttServer> GetAllMqttServers()
        {
            return _mqttServers.Values.ToList();
        }

        /// <summary>
        /// 发布变量数据到MQTT服务器
        /// </summary>
        public async Task PublishVariableDataAsync(MqttAlias variableMqtt, CancellationToken cancellationToken = default)
        {
            await _mqttServiceManager.PublishVariableDataAsync(variableMqtt, cancellationToken);
        }

        /// <summary>
        /// 发布批量变量数据到MQTT服务器
        /// </summary>
        public async Task PublishVariablesDataAsync(List<VariableMqtt> variableMqtts, CancellationToken cancellationToken = default)
        {
            await _mqttServiceManager.PublishVariablesDataAsync(variableMqtts, cancellationToken);
        }

        /// <summary>
        /// 后台服务的核心执行逻辑
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MQTT后台服务正在启动");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _reloadSemaphore.WaitAsync(stoppingToken);

                    if (stoppingToken.IsCancellationRequested) break;

                    // 加载MQTT配置
                    if (!LoadMqttConfigurations())
                    {
                        _logger.LogInformation("加载MQTT配置过程中发生了错误，停止后面的操作");
                        continue;
                    }

                    // 连接MQTT服务器
                    await ConnectMqttServersAsync(stoppingToken);
                    _logger.LogInformation("MQTT后台服务已启动");

                    // 保持运行状态
                    while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MQTT后台服务正在停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MQTT后台服务运行中发生了错误: {ex.Message}");
            }
            finally
            {
                _logger.LogInformation("MQTT后台服务已停止");
            }
        }

        /// <summary>
        /// 加载MQTT配置
        /// </summary>
        private bool LoadMqttConfigurations()
        {
            try
            {
                _logger.LogInformation("开始加载MQTT配置...");
                _mqttServers.Clear();

                // 从数据服务中心获取所有激活的MQTT服务器
                var mqttServers = _appStorageService.MqttServers.Values.ToList();

                foreach (var mqttServer in mqttServers)
                {
                    _mqttServers.TryAdd(mqttServer.Id, mqttServer);
                    _mqttServiceManager.AddMqttServer(mqttServer);
                }

                _logger.LogInformation($"成功加载 {mqttServers.Count} 个MQTT配置");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载MQTT配置时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 连接MQTT服务器列表
        /// </summary>
        private async Task ConnectMqttServersAsync(CancellationToken stoppingToken)
        {
            var connectTasks = _mqttServers.Values
                .Where(m => m.IsActive)
                .Select(mqtt => _mqttServiceManager.ConnectMqttServerAsync(mqtt.Id, stoppingToken));

            await Task.WhenAll(connectTasks);
        }

        /// <summary>
        /// 处理MQTT列表变化
        /// </summary>
        private void HandleMqttListChanged(List<MqttServer> mqtts)
        {
            _logger.LogInformation("MQTT列表发生了变化，正在重新加载数据...");
            _reloadSemaphore.Release();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            _logger.LogInformation("正在释放MQTT后台服务资源...");

            _eventService.OnLoadDataCompleted -= OnLoadDataCompleted;
            _reloadSemaphore?.Dispose();

            base.Dispose();

            _logger.LogInformation("MQTT后台服务资源已释放");
        }
    }
}