using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.Helper;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class OpcUaImportDialogViewModel : ObservableObject
{
    // [ObservableProperty]
    // private string _endpointUrl = "opc.tcp://127.0.0.1:4855"; // 默认值
    //
    // [ObservableProperty]
    // private ObservableCollection<OpcUaNode> _opcUaNodes;
    //
    // [ObservableProperty]
    // private ObservableCollection<Variable> _selectedNodeVariables;
    //
    // public List<Variable> SelectedVariables { get; set; }=new List<Variable>();
    //
    // [ObservableProperty]
    // private bool _selectAllVariables;
    //
    // [ObservableProperty]
    // private bool _isConnected;
    //
    // private Session _session;
    //
    // public OpcUaImportDialogViewModel()
    // {
    //     OpcUaNodes = new ObservableCollection<OpcUaNode>();
    //     SelectedNodeVariables = new ObservableCollection<Variable>();
    //     // Automatically connect when the ViewModel is created
    //     ConnectCommand.Execute(null);
    //     
    // }
    //
    // [RelayCommand]
    // private async Task Connect()
    // {
    //     try
    //     {
    //         // 断开现有连接
    //         if (_session != null && _session.Connected)
    //         {
    //             await _session.CloseAsync();
    //             _session.Dispose();
    //             _session = null;
    //         }
    //
    //         IsConnected = false;
    //         OpcUaNodes.Clear();
    //         SelectedNodeVariables.Clear();
    //
    //         _session = await ServiceHelper.CreateOpcUaSessionAsync(EndpointUrl);
    //
    //         NotificationHelper.ShowSuccess($"已连接到 OPC UA 服务器: {EndpointUrl}");
    //         IsConnected = true;
    //
    //         // 浏览根节点
    //         await BrowseNodes(OpcUaNodes, ObjectIds.ObjectsFolder);
    //     }
    //     catch (Exception ex)
    //     {
    //         IsConnected = false;
    //         NotificationHelper.ShowError($"连接 OPC UA 服务器失败: {EndpointUrl} - {ex.Message}", ex);
    //     }
    // }
    //
    // /// <summary>
    // /// 处理来自服务器的数据变化通知
    // /// </summary>
    // private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    // {
    //     foreach (var value in item.DequeueValues())
    //     {
    //         Console.WriteLine(
    //             $"[通知] {item.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
    //     }
    // }
    //
    // private async Task BrowseNodes(ObservableCollection<OpcUaNode> nodes, NodeId parentNodeId)
    // {
    //     try
    //     {
    //         Opc.Ua.ReferenceDescriptionCollection references;
    //         byte[] continuationPoint = null;
    //
    //         _session.Browse(
    //             null, // RequestHeader
    //             new ViewDescription(),
    //             parentNodeId,
    //             0u,
    //             BrowseDirection.Forward,
    //             Opc.Ua.ReferenceTypeIds.HierarchicalReferences,
    //             true,
    //             (uint)Opc.Ua.NodeClass.Object | (uint)Opc.Ua.NodeClass.Variable,
    //             out continuationPoint,
    //             out references
    //         );
    //
    //         foreach (var rd in references)
    //         {
    //             NodeType nodeType = NodeType.Folder; // 默认是文件夹
    //             if ((rd.NodeClass & NodeClass.Variable) != 0)
    //             {
    //                 nodeType = NodeType.Variable;
    //             }
    //             else if ((rd.NodeClass & NodeClass.Object) != 0)
    //             {
    //                 nodeType = NodeType.Object;
    //             }
    //
    //             var opcUaNode = new OpcUaNode(rd.DisplayName.Text, (NodeId)rd.NodeId, nodeType);
    //             nodes.Add(opcUaNode);
    //
    //             // 如果是文件夹或对象，添加一个虚拟子节点，用于懒加载
    //             if (nodeType == NodeType.Folder || nodeType == NodeType.Object)
    //             {
    //                 opcUaNode.Children.Add(new OpcUaNode("Loading...", NodeId.Null, NodeType.Folder)); // 虚拟节点
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         NlogHelper.Error($"浏览 OPC UA 节点失败: {parentNodeId} - {ex.Message}", ex);
    //         NotificationHelper.ShowError($"浏览 OPC UA 节点失败: {parentNodeId} - {ex.Message}", ex);
    //     }
    // }
    //
    // public async Task LoadNodeVariables(OpcUaNode node)
    // {
    //     if (node.NodeType == NodeType.Variable)
    //     {
    //         // 如果是变量节点，直接显示它
    //         SelectedNodeVariables.Clear();
    //         SelectedNodeVariables.Add(new Variable
    //                                   {
    //                                       Name = node.DisplayName,
    //                                       NodeId = node.NodeId.ToString(),
    //                                       OpcUaNodeId = node.NodeId.ToString(),
    //                                       ProtocolType = ProtocolType.OpcUA,
    //                                       IsActive = true // 默认选中
    //                                   });
    //         return;
    //     }
    //
    //     if (node.IsLoaded || node.IsLoading)
    //     {
    //         return; // 已经加载或正在加载
    //     }
    //
    //     node.IsLoading = true;
    //     node.Children.Clear(); // 清除虚拟节点
    //
    //     try
    //     {
    //         Opc.Ua.ReferenceDescriptionCollection references;
    //         byte[] continuationPoint = null;
    //
    //         _session.Browse(
    //             null, // RequestHeader
    //             new ViewDescription(),
    //             node.NodeId,
    //             0u,
    //             BrowseDirection.Forward,
    //             Opc.Ua.ReferenceTypeIds.HierarchicalReferences,
    //             true,
    //             (uint)Opc.Ua.NodeClass.Object | (uint)Opc.Ua.NodeClass.Variable,
    //             out continuationPoint,
    //             out references
    //         );
    //
    //         foreach (var rd in references)
    //         {
    //             NodeType nodeType = NodeType.Folder;
    //             if ((rd.NodeClass & NodeClass.Variable) != 0)
    //             {
    //                 nodeType = NodeType.Variable;
    //             }
    //             else if ((rd.NodeClass & NodeClass.Object) != 0)
    //             {
    //                 nodeType = NodeType.Object;
    //             }
    //
    //             var opcUaNode = new OpcUaNode(rd.DisplayName.Text, (NodeId)rd.NodeId, nodeType);
    //             node.Children.Add(opcUaNode);
    //
    //             if (nodeType == NodeType.Folder || nodeType == NodeType.Object)
    //             {
    //                 opcUaNode.Children.Add(new OpcUaNode("Loading...", NodeId.Null, NodeType.Folder)); // 虚拟节点
    //             }
    //
    //             // 如果是变量，添加到右侧列表
    //             if (nodeType == NodeType.Variable)
    //             {
    //                 // Read the DataType attribute
    //                 ReadValueId readValueId = new ReadValueId
    //                                           {
    //                                               NodeId = opcUaNode.NodeId,
    //                                               AttributeId = Attributes.DataType,
    //                                               // You might need to specify IndexRange and DataEncoding if dealing with arrays or specific encodings
    //                                           };
    //
    //                 DataValueCollection results;
    //                 DiagnosticInfoCollection diagnosticInfos;
    //
    //                 _session.Read(
    //                     null, // RequestHeader
    //                     0, // MaxAge
    //                     TimestampsToReturn.Source,
    //                     new ReadValueIdCollection { readValueId },
    //                     out results,
    //                     out diagnosticInfos
    //                 );
    //
    //                 string dataType = string.Empty;
    //
    //                 if (results != null && results.Count > 0 && results[0].Value != null)
    //                 {
    //                     // Convert the NodeId of the DataType to a readable string
    //                     NodeId dataTypeNodeId = (NodeId)results[0].Value;
    //                     dataType = _session.NodeCache.GetDisplayText(dataTypeNodeId);
    //                 }
    //
    //                 SelectedNodeVariables.Add(new Variable
    //                                           {
    //                                               Name = opcUaNode.DisplayName,
    //                                               OpcUaNodeId = opcUaNode.NodeId.ToString(),
    //                                               ProtocolType = ProtocolType.OpcUA,
    //                                               IsActive = true, // Default selected
    //                                               DataType = dataType // Assign the read DataType
    //                                           });
    //             }
    //         }
    //
    //         node.IsLoaded = true;
    //     }
    //     catch (Exception ex)
    //     {
    //         NlogHelper.Error($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
    //         NotificationHelper.ShowError($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
    //     }
    //     finally
    //     {
    //         node.IsLoading = false;
    //     }
    // }
    //
    // public ObservableCollection<Variable> GetSelectedVariables()
    // {
    //     return new ObservableCollection<Variable>(SelectedVariables);
    // }
}