using DMS.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// OPC UA服务接口，定义了与OPC UA服务器进行通信所需的方法
    /// </summary>
    public interface IOpcUaService
    {
        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步连接到OPC UA服务器
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task ConnectAsync(string serverUrl);

        /// <summary>
        /// 异步断开与OPC UA服务器的连接
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task DisconnectAsync();

        /// <summary>
        /// 浏览指定节点的子节点
        /// </summary>
        /// <param name="nodeToBrowse">要浏览的节点，如果为null则浏览根节点</param>
        /// <returns>表示异步操作的任务，包含子节点列表</returns>
        Task<List<OpcUaNode>> BrowseNode(OpcUaNode? nodeToBrowse);

        /// <summary>
        /// 订阅单个节点的数据变化
        /// </summary>
        /// <param name="node">要订阅的节点</param>
        /// <param name="onDataChange">数据变化时的回调方法</param>
        /// <param name="publishingInterval">发布间隔（毫秒）</param>
        /// <param name="samplingInterval">采样间隔（毫秒）</param>
        void SubscribeToNode(OpcUaNode node, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500);

        /// <summary>
        /// 订阅多个节点的数据变化
        /// </summary>
        /// <param name="nodes">要订阅的节点列表</param>
        /// <param name="onDataChange">数据变化时的回调方法</param>
        /// <param name="publishingInterval">发布间隔（毫秒）</param>
        /// <param name="samplingInterval">采样间隔（毫秒）</param>
        void SubscribeToNode(List<OpcUaNode> nodes, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500);

        /// <summary>
        /// 取消订阅单个节点
        /// </summary>
        /// <param name="node">要取消订阅的节点</param>
        void UnsubscribeFromNode(OpcUaNode node);

        /// <summary>
        /// 取消订阅多个节点
        /// </summary>
        /// <param name="nodes">要取消订阅的节点列表</param>
        void UnsubscribeFromNode(List<OpcUaNode> nodes);

        /// <summary>
        /// 获取当前已订阅的所有节点
        /// </summary>
        /// <returns>已订阅节点的列表</returns>
        List<OpcUaNode> GetSubscribedNodes();

        /// <summary>
        /// 异步读取单个节点的值
        /// </summary>
        /// <param name="node">要读取的节点</param>
        /// <returns>表示异步操作的任务</returns>
        Task ReadNodeValueAsync(OpcUaNode node);

        /// <summary>
        /// 异步读取多个节点的值
        /// </summary>
        /// <param name="nodes">要读取的节点列表</param>
        /// <returns>表示异步操作的任务</returns>
        Task ReadNodeValuesAsync(List<OpcUaNode> nodes);

        /// <summary>
        /// 异步写入单个节点的值
        /// </summary>
        /// <param name="node">要写入的节点</param>
        /// <param name="value">要写入的值</param>
        /// <returns>表示异步操作的任务，如果写入成功返回true，否则返回false</returns>
        Task<bool> WriteNodeValueAsync(OpcUaNode node, object value);

        /// <summary>
        /// 异步写入多个节点的值
        /// </summary>
        /// <param name="nodesToWrite">要写入的节点及其对应值的字典</param>
        /// <returns>表示异步操作的任务，如果所有写入都成功返回true，否则返回false</returns>
        Task<bool> WriteNodeValuesAsync(Dictionary<OpcUaNode, object> nodesToWrite);
    }
}
