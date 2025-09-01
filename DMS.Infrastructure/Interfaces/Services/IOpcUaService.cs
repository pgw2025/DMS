using DMS.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces.Services
{
    public interface IOpcUaService
    {
        bool IsConnected { get; }
        Task ConnectAsync(string serviceURL);
        Task DisconnectAsync();
        Task<List<OpcUaNode>> BrowseNode(OpcUaNode? nodeToBrowse);
        void SubscribeToNode(OpcUaNode node, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500);
        void SubscribeToNode(List<OpcUaNode> nodes, Action<OpcUaNode> onDataChange, int publishingInterval = 1000, int samplingInterval = 500);
        void UnsubscribeFromNode(OpcUaNode node);
        void UnsubscribeFromNode(List<OpcUaNode> nodes);
        List<OpcUaNode> GetSubscribedNodes();
        Task ReadNodeValueAsync(OpcUaNode node);
        Task ReadNodeValuesAsync(List<OpcUaNode> nodes);
        Task<bool> WriteNodeValueAsync(OpcUaNode node, object value);
        Task<bool> WriteNodeValuesAsync(Dictionary<OpcUaNode, object> nodesToWrite);
    }
}
