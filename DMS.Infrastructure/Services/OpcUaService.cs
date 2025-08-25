using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Helper;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class OpcUaService : IOpcUaService
    {
        private Session? _session;
        private string _serverUrl;

        /// <summary>
        /// 创建 OPC UA 会话
        /// </summary>
        /// <param name="opcUaServerUrl">OPC UA 服务器地址</param>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns></returns>
        public async Task CreateSession( CancellationToken stoppingToken = default)
        {
          

            try
            {
                _session = await OpcUaHelper.CreateOpcUaSessionAsync(_serverUrl, stoppingToken);
                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create OPC UA session: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 连接到 OPC UA 服务器
        /// </summary>
        public async Task ConnectAsync(string opcUaServerUrl, CancellationToken stoppingToken = default)
        {
            _serverUrl = opcUaServerUrl;
            if (string.IsNullOrEmpty(opcUaServerUrl))
            {
                throw new ArgumentException("OPC UA server URL cannot be null or empty.", nameof(opcUaServerUrl));
            }
            if (string.IsNullOrEmpty(_serverUrl))
            {
                throw new InvalidOperationException("Server URL is not set. Please call CreateSession first.");
            }
            
            // 如果已经连接，直接返回
            if (_session?.Connected == true)
            {
                return;
            }

            // 重新创建会话
            await CreateSession( stoppingToken);
        }

       

        /// <summary>
        /// 断开 OPC UA 服务器连接
        /// </summary>
        public void Disconnect()
        {
            if (_session != null)
            {
                try
                {
                    _session.Close();
                }
                catch (Exception ex)
                {
                    // 记录日志但不抛出异常，确保清理工作完成
                    System.Diagnostics.Debug.WriteLine($"Error closing OPC UA session: {ex.Message}");
                }
                finally
                {
                    _session = null;
                }
            }
        }

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="subscriptionName">订阅名称</param>
        /// <returns>创建的订阅</returns>
        public Subscription AddSubscription(string subscriptionName)
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Session is not created. Please call CreateSession first.");
            }

            if (!_session.Connected)
            {
                throw new InvalidOperationException("Session is not connected. Please call Connect first.");
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new ArgumentException("Subscription name cannot be null or empty.", nameof(subscriptionName));
            }

            try
            {
                var subscription = new Subscription(_session.DefaultSubscription)
                {
                    DisplayName = subscriptionName,
                    PublishingInterval = 1000,
                    LifetimeCount = 0,
                    MaxNotificationsPerPublish = 0,
                    PublishingEnabled = true,
                    Priority = 0
                };

                _session.AddSubscription(subscription);
                subscription.Create();

                return subscription;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add subscription '{subscriptionName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 浏览节点
        /// </summary>
        /// <param name="nodeId">起始节点ID</param>
        /// <returns>节点引用列表</returns>
        public IList<ReferenceDescription> BrowseNodes(NodeId nodeId)
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Session is not created. Please call CreateSession first.");
            }

            if (!_session.Connected)
            {
                throw new InvalidOperationException("Session is not connected. Please call Connect first.");
            }

            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            try
            {
                // 使用会话的浏览方法
                var response = _session.Browse(
                    null,
                    null,
                    nodeId,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Unspecified,
                    out var continuationPoint,
                    out var references);

                return references;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to browse nodes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 读取节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>节点值</returns>
        public DataValue ReadValue(NodeId nodeId)
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Session is not created. Please call CreateSession first.");
            }

            if (!_session.Connected)
            {
                throw new InvalidOperationException("Session is not connected. Please call Connect first.");
            }

            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            try
            {
                // 创建读取值集合
                var nodesToRead = new ReadValueIdCollection
                {
                    new ReadValueId
                    {
                        NodeId = nodeId,
                        AttributeId = Attributes.Value
                    }
                };

                // 执行读取操作
                _session.Read(
                    null,
                    0,
                    TimestampsToReturn.Neither,
                    nodesToRead,
                    out var results,
                    out var diagnosticInfos);

                return results[0];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read value from node '{nodeId}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 写入节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public StatusCode WriteValue(NodeId nodeId, object value)
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Session is not created. Please call CreateSession first.");
            }

            if (!_session.Connected)
            {
                throw new InvalidOperationException("Session is not connected. Please call Connect first.");
            }

            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            try
            {
                // 创建写入值集合
                var nodesToWrite = new WriteValueCollection
                {
                    new WriteValue
                    {
                        NodeId = nodeId,
                        AttributeId = Attributes.Value,
                        Value = new DataValue(new Variant(value))
                    }
                };

                // 执行写入操作
                _session.Write(
                    null,
                    nodesToWrite,
                    out var results,
                    out var diagnosticInfos);

                return results[0];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write value to node '{nodeId}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查是否已连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _session?.Connected == true;
        }
    }
}
