using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class OpcUaImportDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _endpointUrl = "opc.tcp://127.0.0.1:4855"; // 默认值

    [ObservableProperty]
    private ObservableCollection<OpcUaNode> _opcUaNodes;

    [ObservableProperty]
    private ObservableCollection<VariableData> _selectedNodeVariables;

    public List<VariableData> SelectedVariables { get; set; }=new List<VariableData>();

    [ObservableProperty]
    private bool _selectAllVariables;

    private Session _session;

    public OpcUaImportDialogViewModel()
    {
        OpcUaNodes = new ObservableCollection<OpcUaNode>();
        SelectedNodeVariables = new ObservableCollection<VariableData>();
    }

    [RelayCommand]
    private async Task Connect()
    {
        try
        {
            // 断开现有连接
            if (_session != null && _session.Connected)
            {
                _session.Close();
                _session.Dispose();
                _session = null;
            }

            /*ApplicationInstance application = new ApplicationInstance
            {
                ApplicationName = "PMSWPF OPC UA Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "PMSWPF.OpcUaClient"
            };

            ApplicationConfiguration config = await application.LoadApplicationConfiguration(false);

            // var config = new ApplicationConfiguration()
            //              {
            //                  ApplicationName = application.ApplicationName,
            //                  ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:OpcUADemoClient",
            //                  ApplicationType = application.ApplicationType,
            //                  SecurityConfiguration = new SecurityConfiguration
            //                                          {
            //                                              ApplicationCertificate = new CertificateIdentifier { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault", SubjectName = application.ApplicationName },
            //                                              TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Certificate Authorities" },
            //                                              TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Applications" },
            //                                              RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/RejectedCertificates" },
            //                                              AutoAcceptUntrustedCertificates = true // 自动接受不受信任的证书 (仅用于测试)
            //                                          },
            //                  TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            //                  ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
            //                  TraceConfiguration = new TraceConfiguration { OutputFilePath = "./Logs/OpcUaClient.log", DeleteOnLoad = true, TraceMasks = Utils.TraceMasks.Error | Utils.TraceMasks.Security }
            //              };
            //
            // bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            // if (!haveAppCertificate)
            // {
            //     throw new Exception("Application instance certificate invalid!");
            // }
            //
            // EndpointDescription selectedEndpoint
            //     = CoreClientUtils.SelectEndpoint(application.ApplicationConfiguration, EndpointUrl, false);
            // EndpointConfiguration endpointConfiguration
            //     = EndpointConfiguration.Create(application.ApplicationConfiguration);
            // ConfiguredEndpoint configuredEndpoint
            //     = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
             // var config = new ApplicationConfiguration()
             //             {
             //                 ApplicationName = application.ApplicationName,
             //                 ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:OpcUADemoClient",
             //                 ApplicationType = application.ApplicationType,
             //                 SecurityConfiguration = new SecurityConfiguration
             //                                         {
             //                                             ApplicationCertificate = new CertificateIdentifier { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault", SubjectName = application.ApplicationName },
             //                                             TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Certificate Authorities" },
             //                                             TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Applications" },
             //                                             RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/RejectedCertificates" },
             //                                             AutoAcceptUntrustedCertificates = true // 自动接受不受信任的证书 (仅用于测试)
             //                                         },
             //                 TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
             //                 ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
             //                 TraceConfiguration = new TraceConfiguration { OutputFilePath = "./Logs/OpcUaClient.log", DeleteOnLoad = true, TraceMasks = Utils.TraceMasks.Error | Utils.TraceMasks.Security }
             //             };

            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            EndpointDescription selectedEndpoint
                = CoreClientUtils.SelectEndpoint(application.ApplicationConfiguration, EndpointUrl, false);
            EndpointConfiguration endpointConfiguration
                = EndpointConfiguration.Create(application.ApplicationConfiguration);
            ConfiguredEndpoint configuredEndpoint
                = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            _session = await Session.Create(
                config,
                configuredEndpoint,
                false,
                "PMSWPF OPC UA Session",
                60000,
                new UserIdentity(new AnonymousIdentityToken()),
                null);
                */


            // 1. 创建应用程序配置
            var application = new ApplicationInstance
                              {
                                  ApplicationName = "OpcUADemoClient",
                                  ApplicationType = ApplicationType.Client,
                                  ConfigSectionName = "Opc.Ua.Client"
                              };

            var config = new ApplicationConfiguration()
                         {
                             ApplicationName = application.ApplicationName,
                             ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:OpcUADemoClient",
                             ApplicationType = application.ApplicationType,
                             SecurityConfiguration = new SecurityConfiguration
                                                     {
                                                         ApplicationCertificate = new CertificateIdentifier
                                                             {
                                                                 StoreType = "Directory",
                                                                 StorePath
                                                                     = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault",
                                                                 SubjectName = application.ApplicationName
                                                             },
                                                         TrustedIssuerCertificates
                                                             = new CertificateTrustList
                                                               {
                                                                   StoreType = "Directory",
                                                                   StorePath
                                                                       = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Certificate Authorities"
                                                               },
                                                         TrustedPeerCertificates
                                                             = new CertificateTrustList
                                                               {
                                                                   StoreType = "Directory",
                                                                   StorePath
                                                                       = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Applications"
                                                               },
                                                         RejectedCertificateStore
                                                             = new CertificateTrustList
                                                               {
                                                                   StoreType = "Directory",
                                                                   StorePath
                                                                       = "%CommonApplicationData%/OPC Foundation/CertificateStores/RejectedCertificates"
                                                               },
                                                         AutoAcceptUntrustedCertificates = true // 自动接受不受信任的证书 (仅用于测试)
                                                     },
                             TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                             ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                             TraceConfiguration = new TraceConfiguration
                                                  {
                                                      OutputFilePath = "./Logs/OpcUaClient.log", DeleteOnLoad = true,
                                                      TraceMasks = Utils.TraceMasks.Error | Utils.TraceMasks.Security
                                                  }
                         };
            application.ApplicationConfiguration = config;

            // 验证并检查证书
            await config.Validate(ApplicationType.Client);
            await application.CheckApplicationInstanceCertificate(false, 0);

            // 2. 查找并选择端点 (将 useSecurity 设置为 false 以进行诊断)
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(EndpointUrl, false);

            _session = await Session.Create(
                config,
                new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)),
                false,
                "PMSWPF OPC UA Session",
                60000,
                new UserIdentity(new AnonymousIdentityToken()),
                null);

            NotificationHelper.ShowSuccess($"已连接到 OPC UA 服务器: {EndpointUrl}");

            // 浏览根节点
            await BrowseNodes(OpcUaNodes, ObjectIds.ObjectsFolder);
        }
        catch (Exception ex)
        {
            NlogHelper.Error($"连接 OPC UA 服务器失败: {EndpointUrl} - {ex.Message}", ex);
            NotificationHelper.ShowError($"连接 OPC UA 服务器失败: {EndpointUrl} - {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 处理来自服务器的数据变化通知
    /// </summary>
    private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    {
        foreach (var value in item.DequeueValues())
        {
            Console.WriteLine(
                $"[通知] {item.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
        }
    }

    private async Task BrowseNodes(ObservableCollection<OpcUaNode> nodes, NodeId parentNodeId)
    {
        try
        {
            Opc.Ua.ReferenceDescriptionCollection references;
            byte[] continuationPoint = null;

            _session.Browse(
                null, // RequestHeader
                new ViewDescription(),
                parentNodeId,
                0u,
                BrowseDirection.Forward,
                Opc.Ua.ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)Opc.Ua.NodeClass.Object | (uint)Opc.Ua.NodeClass.Variable,
                out continuationPoint,
                out references
            );

            foreach (var rd in references)
            {
                NodeType nodeType = NodeType.Folder; // 默认是文件夹
                if ((rd.NodeClass & NodeClass.Variable) != 0)
                {
                    nodeType = NodeType.Variable;
                }
                else if ((rd.NodeClass & NodeClass.Object) != 0)
                {
                    nodeType = NodeType.Object;
                }

                var opcUaNode = new OpcUaNode(rd.DisplayName.Text, (NodeId)rd.NodeId, nodeType);
                nodes.Add(opcUaNode);

                // 如果是文件夹或对象，添加一个虚拟子节点，用于懒加载
                if (nodeType == NodeType.Folder || nodeType == NodeType.Object)
                {
                    opcUaNode.Children.Add(new OpcUaNode("Loading...", NodeId.Null, NodeType.Folder)); // 虚拟节点
                }
            }
        }
        catch (Exception ex)
        {
            NlogHelper.Error($"浏览 OPC UA 节点失败: {parentNodeId} - {ex.Message}", ex);
            NotificationHelper.ShowError($"浏览 OPC UA 节点失败: {parentNodeId} - {ex.Message}", ex);
        }
    }

    public async Task LoadNodeVariables(OpcUaNode node)
    {
        if (node.NodeType == NodeType.Variable)
        {
            // 如果是变量节点，直接显示它
            SelectedNodeVariables.Clear();
            SelectedNodeVariables.Add(new VariableData
                                      {
                                          Name = node.DisplayName,
                                          NodeId = node.NodeId.ToString(),
                                          OpcUaNodeId = node.NodeId.ToString(),
                                          ProtocolType = ProtocolType.OpcUA,
                                          IsActive = true // 默认选中
                                      });
            return;
        }

        if (node.IsLoaded || node.IsLoading)
        {
            return; // 已经加载或正在加载
        }

        node.IsLoading = true;
        node.Children.Clear(); // 清除虚拟节点

        try
        {
            Opc.Ua.ReferenceDescriptionCollection references;
            byte[] continuationPoint = null;

            _session.Browse(
                null, // RequestHeader
                new ViewDescription(),
                node.NodeId,
                0u,
                BrowseDirection.Forward,
                Opc.Ua.ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)Opc.Ua.NodeClass.Object | (uint)Opc.Ua.NodeClass.Variable,
                out continuationPoint,
                out references
            );

            foreach (var rd in references)
            {
                NodeType nodeType = NodeType.Folder;
                if ((rd.NodeClass & NodeClass.Variable) != 0)
                {
                    nodeType = NodeType.Variable;
                }
                else if ((rd.NodeClass & NodeClass.Object) != 0)
                {
                    nodeType = NodeType.Object;
                }

                var opcUaNode = new OpcUaNode(rd.DisplayName.Text, (NodeId)rd.NodeId, nodeType);
                node.Children.Add(opcUaNode);

                if (nodeType == NodeType.Folder || nodeType == NodeType.Object)
                {
                    opcUaNode.Children.Add(new OpcUaNode("Loading...", NodeId.Null, NodeType.Folder)); // 虚拟节点
                }

                // 如果是变量，添加到右侧列表
                if (nodeType == NodeType.Variable)
                {
                    // Read the DataType attribute
                    ReadValueId readValueId = new ReadValueId
                                              {
                                                  NodeId = opcUaNode.NodeId,
                                                  AttributeId = Attributes.DataType,
                                                  // You might need to specify IndexRange and DataEncoding if dealing with arrays or specific encodings
                                              };

                    DataValueCollection results;
                    DiagnosticInfoCollection diagnosticInfos;

                    _session.Read(
                        null, // RequestHeader
                        0, // MaxAge
                        TimestampsToReturn.Source,
                        new ReadValueIdCollection { readValueId },
                        out results,
                        out diagnosticInfos
                    );

                    string dataType = string.Empty;

                    if (results != null && results.Count > 0 && results[0].Value != null)
                    {
                        // Convert the NodeId of the DataType to a readable string
                        NodeId dataTypeNodeId = (NodeId)results[0].Value;
                        dataType = _session.NodeCache.GetDisplayText(dataTypeNodeId);
                    }

                    SelectedNodeVariables.Add(new VariableData
                                              {
                                                  Name = opcUaNode.DisplayName,
                                                  OpcUaNodeId = opcUaNode.NodeId.ToString(),
                                                  ProtocolType = ProtocolType.OpcUA,
                                                  IsActive = true, // Default selected
                                                  DataType = dataType // Assign the read DataType
                                              });
                }
            }

            node.IsLoaded = true;
        }
        catch (Exception ex)
        {
            NlogHelper.Error($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
            NotificationHelper.ShowError($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
        }
        finally
        {
            node.IsLoading = false;
        }
    }

    public ObservableCollection<VariableData> GetSelectedVariables()
    {
        return new ObservableCollection<VariableData>(SelectedVariables);
    }
}