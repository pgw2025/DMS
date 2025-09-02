using DMS.Core.Enums;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class OpcUaService : IOpcUaService
    {
        private readonly ApplicationConfiguration _config;
        private string? _serverUrl;
        private Session? _session;
        private Subscription? _subscription;
        private readonly Dictionary<NodeId, OpcUaNode> _subscribedNodes = new();

        public bool IsConnected => _session != null && _session.Connected;

        public OpcUaService()
        {
            _config = CreateApplicationConfiguration();
        }

        public async Task ConnectAsync(string serverUrl)
        {
            try
            {
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
                }

                // 创建一个应用程序实例，它代表了客户端应用程序。
                var application = new ApplicationInstance(_config);

                // 检查应用程序实例证书是否存在且有效，如果不存在则会尝试创建。
                bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 2048);
                if (!haveAppCertificate)
                {
                    throw new Exception("应用程序实例证书无效！");
                }

                // 从给定的URL发现并选择一个合适的服务器终结点(Endpoint)。
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(_config, _serverUrl, useSecurity: false);

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
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"连接服务器时发生错误: {ex.Message}");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_session != null)
            {
                await _session.CloseAsync();
                _session = null;
            }
        }

        public async Task<List<OpcUaNode>> BrowseNode(OpcUaNode? nodeToBrowse)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("会话未连接。请在浏览节点前调用ConnectAsync方法。");
            }
            
            // 检查节点是否为null
            if (nodeToBrowse == null)
            {
                throw new ArgumentNullException(nameof(nodeToBrowse), "要浏览的节点不能为null。");
            }
            
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

                // 处理浏览结果
                await ProcessBrowseResults(references, nodeToBrowse, nodes);

                // 如果continuationPoint不为null，说明服务器还有数据未返回，需要循环调用BrowseNext获取
                while (continuationPoint != null)
                {
                    // 调用BrowseNext获取下一批数据
                    _session.BrowseNext(null, false, continuationPoint, out continuationPoint, out references);

                    // 处理后续批次的浏览结果
                    await ProcessBrowseResults(references, nodeToBrowse, nodes);
                }
                
                // 将找到的子节点列表关联到父节点
                nodeToBrowse.Children = nodes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"浏览节点 '{nodeToBrowse.DisplayName}' 时发生错误: {ex.Message}");
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
                return;

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
                    variableNodes.Add(node);
                }

                nodes.Add(node);
            }

            // 批量读取变量节点的数据类型
            if (variableNodes.Any())
            {
                try
                {
                    await ReadNodeDataTypesAsync(variableNodes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"批量读取节点数据类型时发生错误: {ex.Message}");
                }
            }
        }

        public void SubscribeToNode(OpcUaNode node, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500)
        {
            SubscribeToNode(new List<OpcUaNode> { node }, onDataChange, publishingInterval, samplingInterval);
        }

        public void SubscribeToNode(List<OpcUaNode> nodes, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500)
        {
            // 检查会话是否已连接
            if (!IsConnected)
            {
                throw new InvalidOperationException("会话未连接。请在订阅节点前调用ConnectAsync方法。");
            }
            
            // 检查节点列表是否有效
            if (nodes == null || !nodes.Any())
            {
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
                    continue;
                }
                
                // 为每个节点创建一个监视项
                var monitoredItem = CreateMonitoredItem(node, onDataChange, samplingInterval);
                
                // 将创建的监视项添加到待添加列表
                itemsToAdd.Add(monitoredItem);
                // 将节点添加到我们的跟踪字典中
                _subscribedNodes[node.NodeId] = node;
            }

            // 如果有新的监视项要添加
            if (itemsToAdd.Any())
            {
                // 将所有新的监视项批量添加到订阅中
                _subscription.AddItems(itemsToAdd);
                // 将所有挂起的更改（包括订阅属性修改和添加新项）应用到服务器
                _subscription.ApplyChanges();
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
                        // 更新节点对象的值
                        changedNode.Value = notification.Value.Value;
                        // 调用用户提供的回调函数，并传入更新后的节点
                        onDataChange?.Invoke(changedNode);
                    }
                }
            };
            
            return monitoredItem;
        }

        public void UnsubscribeFromNode(OpcUaNode node)
        {
            UnsubscribeFromNode(new List<OpcUaNode> { node });
        }

        public void UnsubscribeFromNode(List<OpcUaNode> nodes)
        {
            // 检查订阅对象和节点列表是否有效
            if (_subscription == null || nodes == null || !nodes.Any())
            {
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
                    // 如果找到，则添加到待移除列表
                    itemsToRemove.Add(item);
                    // 从我们的跟踪字典中移除该节点
                    _subscribedNodes.Remove(node.NodeId);
                }
            }

            // 如果有需要移除的监视项
            if (itemsToRemove.Any())
            {
                // 从订阅中批量移除监视项
                _subscription.RemoveItems(itemsToRemove);
                // 将更改应用到服务器
                _subscription.ApplyChanges();
            }
        }

        public List<OpcUaNode> GetSubscribedNodes()
        {
            return _subscribedNodes.Values.ToList();
        }

        public Task ReadNodeValueAsync(OpcUaNode node)
        {
            return ReadNodeValuesAsync(new List<OpcUaNode> { node });
        }

        public async Task ReadNodeValuesAsync(List<OpcUaNode> nodes)
        {
            if (!IsConnected || nodes == null || !nodes.Any())
            {
                return;
            }

            // 筛选出变量类型的节点，因为只有变量才有值
            var variableNodes = nodes.Where(n => n.NodeClass == NodeClass.Variable).ToList();
            
            // 如果没有需要读取的变量节点，则直接返回
            if (!variableNodes.Any())
            {
                return;
            }

            try
            {
                // 创建一个用于存放读取请求的集合
                var nodesToRead = new ReadValueIdCollection();
                // 创建一个列表，用于在收到响应后按顺序查找对应的OpcUaNode
                var nodeListForLookup = new List<OpcUaNode>();
                
                // 为每个变量节点创建读取请求
                foreach (var node in variableNodes)
                {
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
                    }
                    else
                    {
                        // 如果读取失败，则将状态码作为值，方便调试
                        node.Value = $"({results[i].StatusCode})";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取节点值时发生错误: {ex.Message}");
            }
        }

        public Task<bool> WriteNodeValueAsync(OpcUaNode node, object value)
        {
            var nodesToWrite = new Dictionary<OpcUaNode, object> { { node, value } };
            return WriteNodeValuesAsync(nodesToWrite);
        }

        public async Task<bool> WriteNodeValuesAsync(Dictionary<OpcUaNode, object> nodesToWrite)
        {
            // 检查会话是否连接，以及待写入的节点字典是否有效
            if (!IsConnected || nodesToWrite == null || !nodesToWrite.Any())
            {
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
                    Console.WriteLine($"节点 '{node.DisplayName}' 不是变量类型，无法写入。");
                }
                
                return false;
            }

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
                    Console.WriteLine($"为节点 '{node.DisplayName}' 创建写入值时发生错误: {ex.Message}");
                }
            }

            // 如果没有有效的写入请求，则直接返回
            if (writeValues.Count == 0)
            {
                return false;
            }

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
                // 遍历返回的结果状态码
                for (int i = 0; i < results.Count; i++)
                {
                    // 如果返回的状态码表示失败
                    if (StatusCode.IsBad(results[i]))
                    {
                        allSuccess = false;
                        // 根据索引找到写入失败的节点
                        var failedNode = nodeListForLookup[i];
                        Console.WriteLine($"写入节点 '{failedNode.DisplayName}' 失败: {results[i]}");
                    }
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入节点值时发生错误: {ex.Message}");
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
