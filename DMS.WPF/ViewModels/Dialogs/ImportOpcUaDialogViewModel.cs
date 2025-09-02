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
using System.Collections;
using System.Collections.ObjectModel;
using static Dm.net.buffer.ByteArrayBuffer;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ImportOpcUaDialogViewModel : DialogViewModelBase<List<VariableItemViewModel>>
{
    [ObservableProperty]
    private string _endpointUrl = "opc.tcp://127.0.0.1:4855"; // 默认值

    [ObservableProperty]
    private OpcUaNodeItemViewModel _rootOpcUaNode;

    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _opcUaNodeVariables = new ObservableCollection<VariableItemViewModel>();

    [ObservableProperty]
    private IList _selectedVariables = new ArrayList();
    [ObservableProperty]
    private bool _selectAllVariables;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectButtonText = "连接服务器";

    [ObservableProperty]
    private bool _isConnectButtonEnabled = true;

    [ObservableProperty]
    private OpcUaNodeItemViewModel _currentOpcUaNodeItem;

    private Session _session;

    private readonly IOpcUaService _opcUaService;
    private readonly IMapper _mapper;
    private CancellationTokenSource _cancellationTokenSource;

    public ImportOpcUaDialogViewModel(IOpcUaService opcUaService, IMapper mapper)
    {
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
                IsConnected = true;
                ConnectButtonText = "已连接";
                IsConnectButtonEnabled = false;
            }


            // 浏览根节点

            var childrens = await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(RootOpcUaNode));
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
    private async void SecondaryButton()
    {
        await _opcUaService.DisconnectAsync();

        Close(SelectedVariables.Cast<VariableItemViewModel>().ToList());
    }

    [RelayCommand]
    private async void PrimaryButton()
    {
        await _opcUaService.DisconnectAsync();

        Close(OpcUaNodeVariables.ToList());
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




    public async Task LoadNodeVariables(OpcUaNodeItemViewModel node)
    {

        try
        {
            OpcUaNodeVariables.Clear();

            // 加载节点的子项
            node.IsExpanded = true;
            node.IsSelected = true;
            CurrentOpcUaNodeItem = node;
            await Browse(node);
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"加载 OPC UA 节点变量失败: {node.NodeId} - {ex.Message}", ex);
        }
    }

    private async Task Browse(OpcUaNodeItemViewModel node, bool isScan = false)
    {
        var childrens = await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(node));
        foreach (var children in childrens)
        {
            var opcNodeItem = _mapper.Map<OpcUaNodeItemViewModel>(children);
            if (children.NodeClass == NodeClass.Variable)
            {
                OpcUaNodeVariables.Add(new VariableItemViewModel
                {
                    Name = children.DisplayName, // 修正：使用子节点的显示名称
                    OpcUaNodeId = children.NodeId.ToString(),
                    Protocol = ProtocolType.OpcUa,
                    CSharpDataType = children.DataType,
                    IsActive = true // 默认选中
                });
            }
            else
            {
                if (node.Children.FirstOrDefault(n => n.NodeId == opcNodeItem.NodeId) == null)
                {
                    node.Children.Add(opcNodeItem);
                }

                if (isScan)
                {
                    Browse(opcNodeItem);
                }
            }
        }
    }

    [RelayCommand]
    private async Task FindCurrentNodeVariables()
    {
        try
        {
            if (CurrentOpcUaNodeItem == null)
            {
                NotificationHelper.ShowError($"请先选择左边的节点，然后再查找变量。");
                return;
            }

            OpcUaNodeVariables.Clear();

            // 加载节点的子项
            CurrentOpcUaNodeItem.IsExpanded = true;
            CurrentOpcUaNodeItem.IsSelected = true;

            await Browse(CurrentOpcUaNodeItem, true);
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"加载 OPC UA 节点变量失败: {CurrentOpcUaNodeItem.NodeId} - {ex.Message}", ex);
        }
    }
}