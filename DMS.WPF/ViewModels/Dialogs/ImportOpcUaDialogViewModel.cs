using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Helper;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using DMS.WPF.ViewModels.Items;
using Opc.Ua;
using Opc.Ua.Client;
using System.Collections.ObjectModel;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ImportOpcUaDialogViewModel : DialogViewModelBase<List<VariableItemViewModel>>
{
    [ObservableProperty]
    private string _endpointUrl = "opc.tcp://127.0.0.1:4855"; // 默认值

    [ObservableProperty]
    private OpcUaNodeItemViewModel _rootOpcUaNode;

    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _selectedNodeVariables;

    public List<Variable> SelectedVariables { get; set; } = new List<Variable>();

    [ObservableProperty]
    private bool _selectAllVariables;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectButtonText = "连接服务器";

    [ObservableProperty]
    private bool _isConnectButtonEnabled = true;

    private Session _session;

    private readonly IOpcUaService _opcUaService;
    private readonly IMapper _mapper;
    private CancellationTokenSource _cancellationTokenSource;

    public ImportOpcUaDialogViewModel(IOpcUaService opcUaService,IMapper mapper)
    {
        SelectedNodeVariables = new ObservableCollection<VariableItemViewModel>();
        this._opcUaService = opcUaService;
        this._mapper = mapper;
        RootOpcUaNode = new OpcUaNodeItemViewModel() { DisplayName = "根节点", NodeId = Objects.ObjectsFolder, IsExpanded = true };
        _cancellationTokenSource = new CancellationTokenSource();

    }

    [RelayCommand]
    private async Task Connect()
    {
        try
        {
            // 断开现有连接
            if (!_opcUaService.IsConnected)
            {
                await _opcUaService.ConnectAsync(EndpointUrl);
            }

            if (_opcUaService.IsConnected)
            {
                IsConnected=true;
                ConnectButtonText = "已连接";
                IsConnectButtonEnabled = false;
            }


            // 浏览根节点

           var childrens= await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(RootOpcUaNode));
            RootOpcUaNode.Children = _mapper.Map<ObservableCollection<OpcUaNodeItemViewModel>>(childrens);


        }
        catch (Exception ex)
        {
            IsConnected = false;
            IsConnectButtonEnabled = false;
            ConnectButtonText = "连接服务器";
            NotificationHelper.ShowError($"连接 OPC UA 服务器失败: {EndpointUrl} - {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async void PrimaryButton()
    {
        await _opcUaService.DisconnectAsync();
    }

    [RelayCommand]
    private async void CloseButton()
    {
        await _opcUaService.DisconnectAsync();
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



    private bool _isLoadingNodeVariables = false;
    
    public async Task LoadNodeVariables(OpcUaNodeItemViewModel node)
    {
        // 防止重复加载
        if (_isLoadingNodeVariables)
            return;
            
        _isLoadingNodeVariables = true;
        
        try
        {
            SelectedNodeVariables.Clear();

            if (node.NodeClass == NodeClass.Variable)
            {
                // 如果是变量节点，直接显示它
                SelectedNodeVariables.Add(new VariableItemViewModel
                {
                    Name = node.DisplayName,
                    OpcUaNodeId = node.NodeId.ToString(),
                    Protocol = ProtocolType.OpcUa,
                    IsActive = true // 默认选中
                });
                return;
            }

            // 加载节点的子项
            node.IsExpanded = true;
            node.IsSelected = true;
            
            var childrens = await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(node));
            foreach (var children in childrens)
            {
                var opcNodeItem = _mapper.Map<OpcUaNodeItemViewModel>(children);
                if (children.NodeClass == NodeClass.Variable)
                {
                    SelectedNodeVariables.Add(new VariableItemViewModel
                    {
                        Name = children.DisplayName, // 修正：使用子节点的显示名称
                        OpcUaNodeId = children.NodeId.ToString(),
                        Protocol = ProtocolType.OpcUa,
                        IsActive = true // 默认选中
                    });
                }
                else
                {
                    node.Children.Add(opcNodeItem);
                }
            }
        }
        catch (Exception ex)
        {
            NlogHelper.Error($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
            NotificationHelper.ShowError($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
        }
        finally
        {
            _isLoadingNodeVariables = false;
        }
    }


}