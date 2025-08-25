using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    public interface IOpcUaService
    {

        /// <summary>
        /// 连接到 OPC UA 服务器（异步）
        /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns></returns>
        public Task ConnectAsync(string opcUaServerUrl,CancellationToken stoppingToken = default);



        /// <summary>
        /// 断开 OPC UA 服务器连接
        /// </summary>
        public void Disconnect();

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="subscriptionName">订阅名称</param>
        /// <returns>创建的订阅</returns>
        public Subscription AddSubscription(string subscriptionName);

        /// <summary>
        /// 浏览节点
        /// </summary>
        /// <param name="nodeId">起始节点ID</param>
        /// <returns>节点引用列表</returns>
        public IList<ReferenceDescription> BrowseNodes(NodeId nodeId);

        /// <summary>
        /// 读取节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>节点值</returns>
        public DataValue ReadValue(NodeId nodeId);

        /// <summary>
        /// 写入节点值
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public StatusCode WriteValue(NodeId nodeId, object value);

        /// <summary>
        /// 检查是否已连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnected();
    }
}