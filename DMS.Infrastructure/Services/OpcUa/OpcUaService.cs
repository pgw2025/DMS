using DMS.Core.Enums;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace DMS.Infrastructure.Services.OpcUa
{
    public class OpcUaService : IOpcUaService
    {
        private readonly ApplicationConfiguration _config;
        private readonly ILogger<OpcUaService> _logger;
        private string? _serverUrl;
        private Session? _session;
        private Subscription? _subscription;
        private readonly Dictionary<NodeId, OpcUaNode> _subscribedNodes = new();

        public OpcUaService(ILogger<OpcUaService> logger = null)
        {
            _logger = logger;
            _config = CreateApplicationConfiguration();
        }

        public bool IsConnected => _session != null && _session.Connected;

        public OpcUaService()
        {
            _config = CreateApplicationConfiguration();
        }

        public async Task ConnectAsync(string serverUrl)
        {
            try
            {
                _logger?.LogInformation("正在连接到OPC UA服务器: {ServerUrl}", serverUrl);
                
                // 保存服务器URL
                _serverUrl = serverUrl;
                
                // 验证客户端应用程序配置的有效性。
                await _config.Validate(ApplicationType.Client);

                // 如果配置为自动接受不受信任的证书，则设置证书验证回调。
                // 这在开发和测试中很方便，但在生产环境中应谨慎使用。
                if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _config.CertificateValidator.CertificateValidation += (s, e) => 
                    {
                        e.Accept = e.Error.StatusCode == StatusCodes.BadCertificateUntrusted;
                    };
                    _logger?.LogDebug("已设置证书验证回调，自动接受不受信任的证书");
                }
                else
                {
                    _logger?.LogDebug("不自动接受不受信任的证书");
                }

                // 创建一个应用程序实例，它代表了客户端应用程序。
                var application = new ApplicationInstance(_config);

                // 检查应用程序实例证书是否存在且有效，如果不存在则会尝试创建。
                bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 2048);
                if (!haveAppCertificate)
                {
                    _logger?.LogError("应用程序实例证书无效！");
                    throw new Exception("应用程序实例证书无效！");
                }
                else
                {
                    _logger?.LogDebug("应用程序实例证书有效");
                }

                // 从给定的URL发现并选择一个合适的服务器终结点(Endpoint)。
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(_config, _serverUrl, useSecurity: false);
                _logger?.LogDebug("已选择终结点: {Endpoint}", selectedEndpoint.EndpointUrl);

                // 创建到服务器的会话。会话管理客户端和服务器之间的所有通信。
                _session = await Session.Create(
                    _config, // 应用程序配置
                    new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(_config)), // 要连接的已配置端点
                    false, // 不更新服务器端点
                    "OpcUaDemo Session", // 会话名称
                    60000, // 会话超时时间（毫秒）
                    null, // 用户身份验证令牌，此处为匿名
                    null // 首选区域设置
                );
                
                _logger?.LogInformation("成功连接到OPC UA服务器: {ServerUrl}", serverUrl);
            }
            catch (Exception ex)
            { 
                _logger?.LogError(ex, "连接服务器时发生错误: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            _logger?.LogInformation("正在断开与OPC UA服务器的连接");
            
            if (_session != null)
            {
                // 取消所有订阅
                if (_subscription != null)
                {
                    try
                    {
                        _logger?.LogDebug("正在删除订阅");
                        // 删除服务器上的订阅
                        _subscription.Delete(true);
                        // 从会话中移除订阅
                        _session.RemoveSubscription(_subscription);
                        // 释放订阅资源
                        _subscription.Dispose();
                        _subscription = null;
                        _logger?.LogDebug("已删除订阅");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "取消订阅时发生错误: {ErrorMessage}", ex.Message);
                        // 即使取消订阅失败，也继续关闭会话
                    }
                }
                
                // 清理订阅节点跟踪字典
                _subscribedNodes.Clear();
                _logger?.LogDebug("已清理订阅节点跟踪字典");

                // 关闭会话
                await _session.CloseAsync();
                _session = null;
                _logger?.LogInformation("已断开与OPC UA服务器的连接");
            }
            else
            {
                _logger?.LogWarning("尝试断开连接，但会话为null");
            }
        }

        public async Task<List<OpcUaNode>> BrowseNode(OpcUaNode? nodeToBrowse)
        {
            if (!IsConnected)
            {
                _logger?.LogWarning("会话未连接。请在浏览节点前调用ConnectAsync方法。");
                throw new InvalidOperationException("会话未连接。请在浏览节点前调用ConnectAsync方法。");
            }
            
            // 检查节点是否为null
            if (nodeToBrowse == null)
            {
                _logger?.LogWarning("要浏览的节点不能为null");
                throw new ArgumentNullException(nameof(nodeToBrowse), "要浏览的节点不能为null。");
            }
            
            _logger?.LogDebug("正在浏览节点: {NodeId} ({DisplayName})", nodeToBrowse.NodeId, nodeToBrowse.DisplayName);
            
            var nodes = new List<OpcUaNode>();
            try
            {
                // 存放浏览结果的集合
                ReferenceDescriptionCollection references;
                // 用于处理分页的延续点，如果一次浏览无法返回所有结果，服务器会返回此值
                byte[] continuationPoint;

                // 调用会话的Browse方法来发现服务器地址空间中的节点
                _session.Browse(
                    null, // requestHeader: 使用默认值
                    null, // view: 不指定视图，即在整个地址空间中浏览
                    nodeToBrowse.NodeId, // nodeId: 要浏览的起始节点ID
                    0u, // maxResultsToReturn: 0表示返回所有结果（如果服务器支持）
                    BrowseDirection.Forward, // browseDirection: 向前浏览（子节点）
                    ReferenceTypeIds.HierarchicalReferences, // referenceTypeId: 只获取层级引用，这是最常用的引用类型
                    true, // includeSubtypes: 包含子类型
                    (uint)NodeClass.Object | (uint)NodeClass.Variable, // nodeClassMask: 只返回对象和变量类型的节点
                    out continuationPoint, // continuationPoint: 输出参数，用于处理分页
                    out references); // references: 输出参数，浏览到的节点引用集合

                _logger?.LogDebug("浏览节点 {NodeId} 成功，获得 {Count} 个子节点", nodeToBrowse.NodeId, references?.Count ?? 0);

                // 处理浏览结果
                await ProcessBrowseResults(references, nodeToBrowse, nodes);

                // 如果continuationPoint不为null，说明服务器还有数据未返回，需要循环调用BrowseNext获取
                int pageCount = 0;
                while (continuationPoint != null)
                {
                    pageCount++;
                    _logger?.LogDebug("正在获取节点 {NodeId} 的第 {PageNumber} 页子节点", nodeToBrowse.NodeId, pageCount);
                    
                    // 调用BrowseNext获取下一批数据
                    _session.BrowseNext(null, false, continuationPoint, out continuationPoint, out references);

                    // 处理后续批次的浏览结果
                    await ProcessBrowseResults(references, nodeToBrowse, nodes);
                    
                    _logger?.LogDebug("已处理节点 {NodeId} 的第 {PageNumber} 页，获得 {Count} 个子节点", nodeToBrowse.NodeId, pageCount, references?.Count ?? 0);
                }
                
                _logger?.LogDebug("总共获得了 {TotalCount} 个子节点", nodes.Count);
                
                // 将找到的子节点列表关联到父节点
                nodeToBrowse.Children = nodes;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "浏览节点 '{NodeId}' ({DisplayName}) 时发生错误: {ErrorMessage}", 
                    nodeToBrowse.NodeId, nodeToBrowse.DisplayName, ex.Message);
                throw;
            }
            return nodes;
        }

        /// <summary>
        /// 处理浏览结果
        /// </summary>
        /// <param name="references">浏览到的节点引用集合</param>
        /// <param name="parentNode">父节点</param>
        /// <param name="nodes">节点列表</param>
        private async Task ProcessBrowseResults(ReferenceDescriptionCollection references, OpcUaNode parentNode, List<OpcUaNode> nodes)
        {
            if (references == null)
            {
                _logger?.LogDebug("浏览结果为null");
                return;
            }

            _logger?.LogDebug("正在处理 {Count} 个浏览结果", references.Count);

            // 收集所有变量节点，用于批量读取数据类型
            var variableNodes = new List<OpcUaNode>();

            // 遍历返回的结果
            foreach (var rd in references)
            {
                var node = new OpcUaNode
                {
                    ParentNode = parentNode,
                    NodeId = (NodeId)rd.NodeId,
                    DisplayName = rd.DisplayName.Text,
                    NodeClass = rd.NodeClass
                };

                // 如果是变量节点，添加到列表中稍后批量处理
                if (rd.NodeClass == NodeClass.Variable)
                {
                    // _logger?.LogDebug("发现变量节点: {NodeId} ({DisplayName})", node.NodeId, node.DisplayName);
                    variableNodes.Add(node);
                }
                else
                {
                    // _logger?.LogDebug("发现对象节点: {NodeId} ({DisplayName})", node.NodeId, node.DisplayName);
                }

                nodes.Add(node);
            }

            // 批量读取变量节点的数据类型
            if (variableNodes.Any())
            {
                _logger?.LogDebug("正在批量读取 {Count} 个变量节点的数据类型", variableNodes.Count);
                
                try
                {
                    await ReadNodeDataTypesAsync(variableNodes);
                    _logger?.LogDebug("成功批量读取 {Count} 个变量节点的数据类型", variableNodes.Count);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "批量读取节点数据类型时发生错误: {ErrorMessage}", ex.Message);
                    throw;
                }
            }
        }

        public void SubscribeToNode(OpcUaNode node, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500)
        {
            _logger?.LogDebug("正在订阅单个节点: {NodeId} ({DisplayName})，发布间隔: {PublishingInterval}ms，采样间隔: {SamplingInterval}ms", 
                node.NodeId, node.DisplayName, publishingInterval, samplingInterval);
            SubscribeToNode(new List<OpcUaNode> { node }, onDataChange, publishingInterval, samplingInterval);
        }

        public void SubscribeToNode(List<OpcUaNode> nodes, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500)
        {
            _logger?.LogDebug("正在订阅 {Count} 个节点，发布间隔: {PublishingInterval}ms，采样间隔: {SamplingInterval}ms", 
                nodes?.Count ?? 0, publishingInterval, samplingInterval);

            // 检查会话是否已连接
            if (!IsConnected)
            {
                _logger?.LogWarning("会话未连接。请在订阅节点前调用ConnectAsync方法。");
                throw new InvalidOperationException("会话未连接。请在订阅节点前调用ConnectAsync方法。");
            }
            
            // 检查节点列表是否有效
            if (nodes == null || !nodes.Any())
            {
                _logger?.LogWarning("节点列表为null或为空，无法订阅");
                return;
            }
            
            // 确保订阅对象存在
            EnsureSubscriptionExists(publishingInterval);

            // 创建一个用于存放待添加监视项的列表
            var itemsToAdd = new List<MonitoredItem>();
            
            // 遍历所有请求订阅的节点
            foreach (var node in nodes)
            {
                // 如果节点已经存在于我们的跟踪列表中，则跳过，避免重复订阅
                if (_subscribedNodes.ContainsKey(node.NodeId))
                {
                    _logger?.LogDebug("节点 {NodeId} ({DisplayName}) 已经被订阅，跳过重复订阅", node.NodeId, node.DisplayName);
                    continue;
                }
                
                // 为每个节点创建一个监视项
                var monitoredItem = CreateMonitoredItem(node, onDataChange, samplingInterval);
                
                // 将创建的监视项添加到待添加列表
                itemsToAdd.Add(monitoredItem);
                // 将节点添加到我们的跟踪字典中
                _subscribedNodes.TryAdd(node.NodeId, node);
                _logger?.LogDebug("节点 {NodeId} ({DisplayName}) 已添加到订阅列表", node.NodeId, node.DisplayName);
            }

            // 如果有新的监视项要添加
            if (itemsToAdd.Any())
            {
                _logger?.LogDebug("批量添加 {Count} 个监视项到订阅", itemsToAdd.Count);
                
                // 将所有新的监视项批量添加到订阅中
                _subscription.AddItems(itemsToAdd);
                // 将所有挂起的更改（包括订阅属性修改和添加新项）应用到服务器
                _subscription.ApplyChanges();
                
                _logger?.LogInformation("已成功订阅 {Count} 个新节点", itemsToAdd.Count);
            }
            else
            {
                _logger?.LogDebug("没有新的节点需要订阅");
            }
        }

        /// <summary>
        /// 确保订阅对象存在
        /// </summary>
        /// <param name="publishingInterval">发布间隔</param>
        private void EnsureSubscriptionExists(int publishingInterval)
        {
            // 如果还没有订阅对象，则基于会话的默认设置创建一个新的订阅
            if (_subscription == null)
            {
                _subscription = new Subscription(_session.DefaultSubscription)
                {
                    // 设置服务器向客户端发送通知的速率（毫秒）
                    PublishingInterval = publishingInterval
                };
                // 在会话中添加订阅
                _session.AddSubscription(_subscription);
                // 在服务器上创建订阅
                _subscription.Create();
            }
            // 如果客户端请求的发布间隔与现有订阅不同，则修改订阅
            else if (_subscription.PublishingInterval != publishingInterval)
            {
                _subscription.PublishingInterval = publishingInterval;
            }
        }

        /// <summary>
        /// 创建监视项
        /// </summary>
        /// <param name="node">OPC UA节点</param>
        /// <param name="onDataChange">数据变化回调</param>
        /// <param name="samplingInterval">采样间隔</param>
        /// <returns>监视项</returns>
        private MonitoredItem CreateMonitoredItem(OpcUaNode node, Action<OpcUaNode> onDataChange, int samplingInterval)
        {
            var monitoredItem = new MonitoredItem(_subscription.DefaultItem)
            {
                DisplayName = node.DisplayName,
                StartNodeId = node.NodeId,
                AttributeId = Attributes.Value, // 我们关心的是节点的值属性
                SamplingInterval = samplingInterval // 服务器采样节点值的速率（毫秒）
            };
            
            // 设置数据变化通知的回调函数
            monitoredItem.Notification += (item, e) =>
            {
                // 将通知事件参数转换为MonitoredItemNotification
                if (e.NotificationValue is MonitoredItemNotification notification)
                {
                    // 通过StartNodeId从我们的跟踪字典中找到对应的OpcUaNode对象
                    if (_subscribedNodes.TryGetValue(item.StartNodeId, out var changedNode))
                    {
                        _logger?.LogDebug("节点 {NodeId} ({DisplayName}) 值发生变化: {Value}", 
                            changedNode.NodeId, changedNode.DisplayName, notification.Value.Value);
                        
                        // 更新节点对象的值
                        changedNode.Value = notification.Value.Value;
                        // 调用用户提供的回调函数，并传入更新后的节点
                        onDataChange?.Invoke(changedNode);
                    }
                    else
                    {
                        _logger?.LogWarning("监视项通知: 无法在跟踪字典中找到节点 {NodeId}", item.StartNodeId);
                    }
                }
            };
            
            return monitoredItem;
        }

        public void UnsubscribeFromNode(OpcUaNode node)
        {
            _logger?.LogDebug("正在取消订阅节点: {NodeId} ({DisplayName})", node.NodeId, node.DisplayName);
            UnsubscribeFromNode(new List<OpcUaNode> { node });
        }

        public void UnsubscribeFromNode(List<OpcUaNode> nodes)
        {
            _logger?.LogDebug("正在取消订阅 {Count} 个节点", nodes?.Count ?? 0);
            
            // 检查订阅对象和节点列表是否有效
            if (_subscription == null)
            {
                _logger?.LogWarning("订阅对象为null，无法取消订阅");
                return;
            }
            
            if (nodes == null || !nodes.Any())
            {
                _logger?.LogWarning("节点列表为null或为空，无法取消订阅");
                return;
            }

            var itemsToRemove = new List<MonitoredItem>();
            // 遍历所有请求取消订阅的节点
            foreach (var node in nodes)
            {
                // 在当前订阅中查找与节点ID匹配的监视项
                var item = _subscription.MonitoredItems.FirstOrDefault(m => m.StartNodeId.Equals(node.NodeId));
                if (item != null)
                {
                    _logger?.LogDebug("找到节点 {NodeId} ({DisplayName}) 的监视项，准备移除", node.NodeId, node.DisplayName);
                    // 如果找到，则添加到待移除列表
                    itemsToRemove.Add(item);
                    // 从我们的跟踪字典中移除该节点
                    _subscribedNodes.Remove(node.NodeId);
                }
                else
                {
                    _logger?.LogDebug("节点 {NodeId} ({DisplayName}) 未在监视项中找到，可能已经取消订阅", node.NodeId, node.DisplayName);
                }
            }

            // 如果有需要移除的监视项
            if (itemsToRemove.Any())
            {
                _logger?.LogDebug("批量移除 {Count} 个监视项", itemsToRemove.Count);
                
                // 从订阅中批量移除监视项
                _subscription.RemoveItems(itemsToRemove);
                // 将更改应用到服务器
                _subscription.ApplyChanges();
                
                _logger?.LogInformation("已成功取消订阅 {Count} 个节点", itemsToRemove.Count);
            }
            else
            {
                _logger?.LogDebug("没有找到需要移除的监视项");
            }
        }

        public List<OpcUaNode> GetSubscribedNodes()
        {
            var subscribedNodes = _subscribedNodes.Values.ToList();
            _logger?.LogDebug("获取当前已订阅的节点列表，共 {Count} 个节点", subscribedNodes.Count);
            return subscribedNodes;
        }

        public Task ReadNodeValueAsync(OpcUaNode node)
        {
            _logger?.LogDebug("正在读取单个节点的值: {NodeId} ({DisplayName})", node.NodeId, node.DisplayName);
            return ReadNodeValuesAsync(new List<OpcUaNode> { node });
        }

        public async Task ReadNodeValuesAsync(List<OpcUaNode> nodes)
        {
            if (!IsConnected)
            {
                _logger?.LogWarning("会话未连接，无法读取节点值");
                return;
            }

            if (nodes == null || !nodes.Any())
            {
                _logger?.LogWarning("节点列表为null或为空，无法读取节点值");
                return;
            }
            
            _logger?.LogDebug("正在读取 {Count} 个节点的值", nodes.Count);

            // 筛选出变量类型的节点，因为只有变量才有值
            var variableNodes = nodes.Where(n => n.NodeClass == NodeClass.Variable).ToList();
            
            // 如果没有需要读取的变量节点，则直接返回
            if (!variableNodes.Any())
            {
                _logger?.LogDebug("没有变量类型的节点需要读取值");
                return;
            }

            _logger?.LogDebug("筛选出 {Count} 个变量节点进行读取", variableNodes.Count);

            try
            {
                // 创建一个用于存放读取请求的集合
                var nodesToRead = new ReadValueIdCollection();
                // 创建一个列表，用于在收到响应后按顺序查找对应的OpcUaNode
                var nodeListForLookup = new List<OpcUaNode>();
                
                // 为每个变量节点创建读取请求
                foreach (var node in variableNodes)
                {
                    _logger?.LogDebug("准备读取节点 {NodeId} ({DisplayName}) 的值", node.NodeId, node.DisplayName);
                    
                    // 创建一个ReadValueId，指定要读取的节点ID和属性（值）
                    nodesToRead.Add(new ReadValueId
                    {
                        NodeId = node.NodeId,
                        AttributeId = Attributes.Value
                    });
                    // 将节点添加到查找列表中
                    nodeListForLookup.Add(node);
                }

                // 异步调用Read方法，批量读取所有节点的值
                var response = await _session.ReadAsync(
                    null, // RequestHeader, 使用默认值
                    0,    // maxAge, 0表示直接从设备读取最新值，而不是从缓存读取
                    TimestampsToReturn.Neither, // TimestampsToReturn, 表示我们不关心值的时间戳
                    nodesToRead, // ReadValueIdCollection, 要读取的节点和属性的集合
                    default // CancellationToken
                );

                // 获取响应中的结果和诊断信息
                var results = response.Results;
                var diagnosticInfos = response.DiagnosticInfos;

                // 验证响应，确保请求成功
                ClientBase.ValidateResponse(results, nodesToRead);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

                // 遍历返回的结果
                for (int i = 0; i < results.Count; i++)
                {
                    // 根据索引找到对应的OpcUaNode
                    var node = nodeListForLookup[i];
                    // 检查状态码，确保读取成功
                    if (StatusCode.IsGood(results[i].StatusCode))
                    {
                        // 更新节点的值
                        node.Value = results[i].Value;
                        _logger?.LogDebug("成功读取节点 {NodeId} ({DisplayName}) 的值: {Value}", node.NodeId, node.DisplayName, results[i].Value);
                    }
                    else
                    {
                        // 如果读取失败，则将状态码作为值，方便调试
                        node.Value = $"({results[i].StatusCode})";
                        _logger?.LogWarning("读取节点 {NodeId} ({DisplayName}) 失败，状态码: {StatusCode}", node.NodeId, node.DisplayName, results[i].StatusCode);
                    }
                }
                
                _logger?.LogInformation("成功读取 {SuccessCount}/{TotalCount} 个节点的值", 
                    results.Count(r => StatusCode.IsGood(r.StatusCode)), results.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "读取节点值时发生错误: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public Task<bool> WriteNodeValueAsync(OpcUaNode node, object value)
        {
            _logger?.LogDebug("正在写入单个节点的值: {NodeId} ({DisplayName}), 值: {Value}", 
                node.NodeId, node.DisplayName, value);
            var nodesToWrite = new Dictionary<OpcUaNode, object> { { node, value } };
            return WriteNodeValuesAsync(nodesToWrite);
        }

        public async Task<bool> WriteNodeValuesAsync(Dictionary<OpcUaNode, object> nodesToWrite)
        {
            _logger?.LogDebug("正在写入 {Count} 个节点的值", nodesToWrite?.Count ?? 0);

            // 检查会话是否连接，以及待写入的节点字典是否有效
            if (!IsConnected)
            {
                _logger?.LogWarning("会话未连接，无法写入节点值");
                return false;
            }

            if (nodesToWrite == null || !nodesToWrite.Any())
            {
                _logger?.LogWarning("节点写入字典为null或为空，无法写入节点值");
                return false;
            }

            // 筛选出变量类型的节点，因为只能向变量类型的节点写入值
            var variableNodesToWrite = nodesToWrite
                .Where(entry => entry.Key.NodeClass == NodeClass.Variable)
                .ToList();

            // 如果没有有效的写入请求，则直接返回
            if (!variableNodesToWrite.Any())
            {
                // 输出非变量节点的警告信息
                var nonVariableNodes = nodesToWrite
                    .Where(entry => entry.Key.NodeClass != NodeClass.Variable)
                    .Select(entry => entry.Key);
                    
                foreach (var node in nonVariableNodes)
                {
                    _logger?.LogWarning("节点 '{DisplayName}' 不是变量类型，无法写入。", node.DisplayName);
                }
                
                return false;
            }

            _logger?.LogDebug("筛选出 {Count} 个变量节点进行写入", variableNodesToWrite.Count);

            // 创建一个用于存放写入请求的集合
            var writeValues = new WriteValueCollection();
            // 创建一个列表，用于在收到响应后按顺序查找对应的OpcUaNode，以进行错误报告
            var nodeListForLookup = new List<OpcUaNode>();

            // 为每个变量节点创建写入请求
            foreach (var entry in variableNodesToWrite)
            {
                var node = entry.Key;
                var value = entry.Value;

                try
                {
                    _logger?.LogDebug("准备写入节点 {NodeId} ({DisplayName}) 的值: {Value}", node.NodeId, node.DisplayName, value);

                    // 创建一个WriteValue对象，它封装了写入操作的所有信息
                    var writeValue = new WriteValue
                    {
                        NodeId = node.NodeId, // 指定要写入的节点ID
                        AttributeId = Attributes.Value, // 指定要写入的是节点的Value属性
                        // 将要写入的值封装在DataValue和Variant中
                        // Variant可以处理各种数据类型
                        Value = new DataValue(new Variant(value))
                    };
                    writeValues.Add(writeValue);
                    // 将节点添加到查找列表中，保持与writeValues集合的顺序一致
                    nodeListForLookup.Add(node);
                }
                catch (Exception ex)
                {
                    // 处理在创建写入值时可能发生的异常（例如，值类型不兼容）
                    _logger?.LogError(ex, "为节点 '{DisplayName}' 创建写入值时发生错误: {ErrorMessage}", node.DisplayName, ex.Message);
                }
            }

            // 如果没有有效的写入请求，则直接返回
            if (writeValues.Count == 0)
            {
                _logger?.LogWarning("没有有效的写入请求");
                return false;
            }

            _logger?.LogDebug("准备批量写入 {Count} 个节点的值", writeValues.Count);

            try
            {
                // 异步调用Write方法，将所有写入请求批量发送到服务器
                var response = await _session.WriteAsync(
                    null, // RequestHeader, 使用默认值
                    writeValues, // WriteValueCollection, 要写入的节点和值的集合
                    default // CancellationToken
                );

                // 获取响应中的结果和诊断信息
                var results = response.Results;
                var diagnosticInfos = response.DiagnosticInfos;

                // 验证响应，确保请求被服务器正确处理
                ClientBase.ValidateResponse(results, writeValues);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, writeValues);

                bool allSuccess = true;
                int successCount = 0;
                int failureCount = 0;
                
                // 遍历返回的结果状态码
                for (int i = 0; i < results.Count; i++)
                {
                    // 如果返回的状态码表示失败
                    if (StatusCode.IsBad(results[i]))
                    {
                        allSuccess = false;
                        failureCount++;
                        // 根据索引找到写入失败的节点
                        var failedNode = nodeListForLookup[i];
                        _logger?.LogError("写入节点 '{DisplayName}' 失败: {StatusCode}", failedNode.DisplayName, results[i]);
                    }
                    else
                    {
                        successCount++;
                        var successfulNode = nodeListForLookup[i];
                        _logger?.LogDebug("成功写入节点 {NodeId} ({DisplayName}) 的值", successfulNode.NodeId, successfulNode.DisplayName);
                    }
                }

                _logger?.LogInformation("批量写入完成: 成功 {SuccessCount} 个，失败 {FailureCount} 个", successCount, failureCount);

                return allSuccess;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "写入节点值时发生错误: {ErrorMessage}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 批量读取节点的数据类型名称
        /// </summary>
        /// <param name="nodes">节点列表</param>
        private async Task ReadNodeDataTypesAsync(List<OpcUaNode> nodes)
        {
            try
            {
                // 创建一个用于存放读取请求的集合
                var nodesToRead = new ReadValueIdCollection();
                // 创建一个列表，用于在收到响应后按顺序查找对应的OpcUaNode
                var nodeListForLookup = new List<OpcUaNode>();

                // 为每个节点创建读取数据类型的请求
                foreach (var node in nodes)
                {
                    if (node.NodeId != null)
                    {
                        // 创建一个ReadValueId，指定要读取的节点ID和属性（数据类型）
                        nodesToRead.Add(new ReadValueId
                        {
                            NodeId = node.NodeId,
                            AttributeId = Attributes.DataType
                        });
                        // 将节点添加到查找列表中
                        nodeListForLookup.Add(node);
                    }
                }

                // 如果没有需要读取的节点，则直接返回
                if (nodesToRead.Count == 0)
                {
                    return;
                }

                // 调用Read方法，批量读取节点的数据类型
                var response = await _session.ReadAsync(
                    null, // RequestHeader, 使用默认值
                    0,    // maxAge, 0表示直接从设备读取最新值，而不是从缓存读取
                    TimestampsToReturn.Neither, // TimestampsToReturn, 表示我们不关心值的时间戳
                    nodesToRead, // ReadValueIdCollection, 要读取的节点和属性的集合
                    default // CancellationToken
                );

                // 获取响应中的结果
                var results = response.Results;

                // 验证响应，确保请求成功
                ClientBase.ValidateResponse(results, nodesToRead);

                // 遍历返回的结果
                for (int i = 0; i < results.Count; i++)
                {
                    // 根据索引找到对应的OpcUaNode
                    var node = nodeListForLookup[i];
                    
                    // 检查状态码，确保读取成功
                    if (StatusCode.IsGood(results[i].StatusCode))
                    {
                        // 获取数据类型NodeId
                        if (results[i].Value is NodeId dataTypeId)
                        {
                            // 尝试获取数据类型的友好名称
                            node.DataType = GetDataTypeName(dataTypeId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"批量读取节点数据类型时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取数据类型的友好名称
        /// </summary>
        /// <param name="dataTypeId">数据类型NodeId</param>
        /// <returns>数据类型的友好名称</returns>
        private DataType GetDataTypeName(NodeId dataTypeId)
        {
            if (dataTypeId == null)
                return DataType.Unknown;

            // 使用OPC UA内置的类型映射
            switch (dataTypeId.Identifier.ToString())
            {
                case "1": return DataType.Bool;       // Boolean
                case "2": return DataType.SByte;      // SByte
                case "3": return DataType.Byte;       // Byte
                case "4": return DataType.Short;      // Int16
                case "5": return DataType.UShort;     // UInt16
                case "6": return DataType.Int;        // Int32
                case "7": return DataType.UInt;       // UInt32
                case "8": return DataType.Long;       // Int64
                case "9": return DataType.ULong;      // UInt64
                case "10": return DataType.Float;     // Float
                case "11": return DataType.Double;    // Double
                case "12": return DataType.String;    // String
                case "13": return DataType.DateTime;  // DateTime
                case "14": return DataType.Guid;      // Guid
                case "15": return DataType.ByteArray; // ByteString
                case "16": return DataType.Object;    // XmlElement
                case "17": return DataType.Object;    // NodeId
                case "18": return DataType.Object;    // ExpandedNodeId
                case "19": return DataType.Object;    // StatusCode
                case "20": return DataType.Object;    // QualifiedName
                case "21": return DataType.Object;    // LocalizedText
                case "22": return DataType.Object;    // ExtensionObject
                case "23": return DataType.Object;    // DataValue
                case "24": return DataType.Object;    // Variant
                case "25": return DataType.Object;    // DiagnosticInfo
                default:
                    // 对于自定义数据类型，返回Unknown
                    return DataType.Unknown;
            }
        }

        private ApplicationConfiguration CreateApplicationConfiguration()
        {
            return new ApplicationConfiguration()
            {
                // 应用程序的名称，会显示在服务器端
                ApplicationName = "OpcUaDemoClient",
                // 应用程序的唯一标识符URI
                ApplicationUri = Utils.Format("urn:{0}:OpcUaDemoClient", System.Net.Dns.GetHostName()),
                // 应用程序类型为客户端
                ApplicationType = ApplicationType.Client,
                // 安全相关的配置
                SecurityConfiguration = new SecurityConfiguration
                {
                    // 应用程序实例证书的存储位置和主题名称
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "OpcUaDemoClient" },
                    // 受信任的证书颁发机构的证书存储
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    // 受信任的对等（服务器）证书的存储
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    // 被拒绝的证书的存储
                    RejectedCertificateStore = new CertificateStoreIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    // 自动接受不受信任的证书（仅建议在开发环境中使用）
                    AutoAcceptUntrustedCertificates = true
                },
                // 传输配置（例如，缓冲区大小）
                TransportConfigurations = new TransportConfigurationCollection(),
                // 传输配额（例如，操作超时时间）
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                // 客户端特定的配置（例如，默认会话超时）
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                // 跟踪和日志记录的配置
                TraceConfiguration = new TraceConfiguration()
            };
        }
    }
}
