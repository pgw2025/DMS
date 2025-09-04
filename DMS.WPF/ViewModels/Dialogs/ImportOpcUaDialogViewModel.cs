using System.Collections;
using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Items;
using Opc.Ua;
using Opc.Ua.Client;

namespace DMS.WPF.ViewModels.Dialogs;

/// <summary>
/// OPC UA导入对话框的视图模型
/// 负责处理OPC UA服务器连接、节点浏览和变量导入等功能
/// </summary>
public partial class ImportOpcUaDialogViewModel : DialogViewModelBase<List<VariableItemViewModel>>, IDisposable
{
    /// <summary>
    /// OPC UA服务器端点URL
    /// </summary>
    [ObservableProperty]
    private string _endpointUrl = "opc.tcp://127.0.0.1:4855"; // 默认值

    /// <summary>
    /// OPC UA根节点
    /// </summary>
    [ObservableProperty]
    private OpcUaNodeItemViewModel _rootOpcUaNode;

    /// <summary>
    /// 当前选中节点下的所有变量集合
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _opcUaNodeVariables = new();

    /// <summary>
    /// 用户选择的变量列表
    /// </summary>
    [ObservableProperty]
    private IList _selectedVariables = new ArrayList();

    /// <summary>
    /// 是否全选变量
    /// </summary>
    [ObservableProperty]
    private bool _selectAllVariables;

    /// <summary>
    /// 是否已连接到OPC UA服务器
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    /// <summary>
    /// 连接按钮显示文本
    /// </summary>
    [ObservableProperty]
    private string _connectButtonText = "连接服务器";

    /// <summary>
    /// 连接按钮是否可用
    /// </summary>
    [ObservableProperty]
    private bool _isConnectButtonEnabled = true;

    /// <summary>
    /// 当前选中的OPC UA节点
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FindCurrentNodeVariablesCommand))] // 当选中节点改变时通知查找变量命令更新可执行状态
    private OpcUaNodeItemViewModel _selectedNode;

    /// <summary>
    /// OPC UA服务接口实例
    /// </summary>
    private readonly IOpcUaService _opcUaService;
    
    /// <summary>
    /// 对象映射器实例
    /// </summary>
    private readonly IMapper _mapper;
    
    /// <summary>
    /// 取消令牌源，用于取消长时间运行的操作
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    /// <summary>
    /// 通知服务实例
    /// </summary>
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 构造函数
    /// 初始化ImportOpcUaDialogViewModel实例
    /// </summary>
    /// <param name="opcUaService">OPC UA服务接口实例</param>
    /// <param name="mapper">对象映射器实例</param>
    /// <param name="notificationService">通知服务实例</param>
    public ImportOpcUaDialogViewModel(IOpcUaService opcUaService, IMapper mapper, INotificationService notificationService)
    {
        _opcUaService = opcUaService;
        _mapper = mapper;
        _notificationService = notificationService;
        // 初始化根节点
        RootOpcUaNode = new OpcUaNodeItemViewModel() { DisplayName = "根节点", NodeId = Objects.ObjectsFolder, IsExpanded = true };
        // 初始化取消令牌源
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// 查找当前节点变量命令的可执行条件
    /// 只有在已连接且有选中节点时才可执行
    /// </summary>
    public bool CanFindCurrentNodeVariables => IsConnected && SelectedNode != null;

    /// <summary>
    /// 连接到OPC UA服务器命令
    /// 负责建立与OPC UA服务器的连接并加载根节点信息
    /// </summary>
    [RelayCommand]
    private async Task Connect()
    {
        try
        {
            // 更新UI状态：禁用连接按钮并显示连接中状态
            IsConnectButtonEnabled = false;
            ConnectButtonText = "连接中...";

            // 异步连接到OPC UA服务器
            await _opcUaService.ConnectAsync(EndpointUrl);

            // 检查连接是否成功建立
            if (_opcUaService.IsConnected)
            {
                // 更新连接状态
                IsConnected = true;
                ConnectButtonText = "已连接";

                // 浏览根节点并加载其子节点
                var children = await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(RootOpcUaNode));
                RootOpcUaNode.Children = _mapper.Map<ObservableCollection<OpcUaNodeItemViewModel>>(children);
            }
        }
        // 处理特定异常类型提供更友好的用户提示
        catch (UnauthorizedAccessException ex)
        {
            _notificationService.ShowError($"连接被拒绝，请检查用户名和密码: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            _notificationService.ShowError($"连接超时，请检查服务器地址和网络连接: {ex.Message}");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"连接 OPC UA 服务器失败: {EndpointUrl} - {ex.Message}", ex);
        }
        finally
        {
            // 确保按钮状态正确更新
            // 如果连接失败，恢复按钮为可用状态
            if (!IsConnected)
            {
                IsConnectButtonEnabled = true;
                ConnectButtonText = "连接服务器";
            }
        }
    }

    /// <summary>
    /// 次要按钮命令（取消导入）
    /// 负责断开与OPC UA服务器的连接并关闭对话框，返回用户选择的变量
    /// </summary>
    [RelayCommand]
    private async Task SecondaryButton()
    {
        try
        {
            // 断开与OPC UA服务器的连接
            await _opcUaService.DisconnectAsync();
            // 关闭对话框并返回用户选择的变量列表
            Close(SelectedVariables.Cast<VariableItemViewModel>().ToList());
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"断开连接时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 主要按钮命令（确认导入）
    /// 负责断开与OPC UA服务器的连接并关闭对话框，返回所有加载的变量
    /// </summary>
    [RelayCommand]
    private async Task PrimaryButton()
    {
        try
        {
            // 断开与OPC UA服务器的连接
            await _opcUaService.DisconnectAsync();
            // 关闭对话框并返回所有加载的变量
            Close(OpcUaNodeVariables.ToList());
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"断开连接时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 关闭按钮命令
    /// 负责断开与OPC UA服务器的连接
    /// </summary>
    [RelayCommand]
    private async Task CloseButton()
    {
        try
        {
            // 断开与OPC UA服务器的连接
            await _opcUaService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"断开连接时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 查找当前节点变量命令
    /// 根据当前选中的节点查找其下的所有变量
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFindCurrentNodeVariables))]
    private async Task FindCurrentNodeVariables()
    {
        try
        {
            // 检查是否有选中的节点
            if (SelectedNode == null)
            {
                _notificationService.ShowError("请先选择左边的节点，然后再查找变量。");
                return;
            }

            // 清空当前变量列表
            OpcUaNodeVariables.Clear();

            // 设置选中节点的状态
            SelectedNode.IsExpanded = true;
            SelectedNode.IsSelected = true;

            // 异步浏览节点变量（递归模式）
            await BrowseNodeVariablesAsync(SelectedNode, true);
        }
        catch (OperationCanceledException)
        {
            // 处理用户取消操作的情况
            _notificationService.ShowInfo("操作已被取消");
        }
        catch (Exception ex)
        {
            // 处理其他异常情况
            _notificationService.ShowError($"加载 OPC UA 节点变量失败: {SelectedNode?.NodeId} - {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 加载节点变量方法
    /// 根据指定节点加载其下的所有变量
    /// </summary>
    /// <param name="node">要加载变量的OPC UA节点</param>
    public async Task LoadNodeVariables(OpcUaNodeItemViewModel node)
    {
        try
        {
            // 清空当前变量列表
            OpcUaNodeVariables.Clear();

            // 设置节点状态
            node.IsExpanded = true;
            node.IsSelected = true;
            // 更新选中节点
            SelectedNode = node;

            // 异步浏览节点变量（非递归模式）
            await BrowseNodeVariablesAsync(node);
        }
        catch (OperationCanceledException)
        {
            // 处理用户取消操作的情况
            _notificationService.ShowInfo("操作已被取消");
        }
        catch (Exception ex)
        {
            // 处理其他异常情况
            _notificationService.ShowError($"加载 OPC UA 节点变量失败: {node?.NodeId} - {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 浏览节点变量异步方法
    /// 递归或非递归地浏览指定节点下的所有变量
    /// </summary>
    /// <param name="node">要浏览的节点</param>
    /// <param name="isRecursive">是否递归浏览子节点</param>
    private async Task BrowseNodeVariablesAsync(OpcUaNodeItemViewModel node, bool isRecursive = false)
    {
        // 参数有效性检查
        if (node == null) return;

        try
        {
            // 检查是否有取消请求
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

            // 异步浏览节点获取子节点列表
            var children = await _opcUaService.BrowseNode(_mapper.Map<OpcUaNode>(node));
            
            // 再次检查是否有取消请求
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

            // 遍历所有子节点
            foreach (var child in children)
            {
                // 映射子节点为视图模型对象
                var nodeItem = _mapper.Map<OpcUaNodeItemViewModel>(child);

                // 判断节点类型是否为变量
                if (child.NodeClass == NodeClass.Variable)
                {
                    // 创建并添加变量项到变量列表
                    OpcUaNodeVariables.Add(new VariableItemViewModel
                    {
                        Name = child.DisplayName,           // 变量名称
                        OpcUaNodeId = child.NodeId.ToString(), // OPC UA节点ID
                        Protocol = ProtocolType.OpcUa,       // 协议类型
                        DataType = child.DataType,    // C#数据类型
                        IsActive = true                      // 默认激活状态
                    });
                }
                // 如果是递归模式且节点不是变量，则递归浏览子节点
                else if (isRecursive)
                {
                    // 递归浏览子节点
                    await BrowseNodeVariablesAsync(nodeItem, true);
                }
                // 非递归模式下，将非变量节点添加到节点树中
                else
                {
                    // 避免重复添加相同节点
                    if (!node.Children.Any(n => n.NodeId == nodeItem.NodeId))
                    {
                        node.Children.Add(nodeItem);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 处理取消操作
            _notificationService.ShowInfo("节点浏览操作已被取消");
            throw; // 重新抛出异常以保持调用链
        }
        catch (Exception ex)
        {
            // 记录浏览节点失败的日志
            _notificationService.ShowError($"浏览节点失败: {node.NodeId} - {ex.Message}", ex);
            throw; // 重新抛出异常以保持调用链
        }
    }

    /// <summary>
    /// 处理来自服务器的数据变化通知
    /// 当监视的OPC UA节点数据发生变化时会被调用
    /// </summary>
    /// <param name="item">发生变化的监视项</param>
    /// <param name="e">监视项通知事件参数</param>
    private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    {
        // 遍历所有变化的值
        foreach (var value in item.DequeueValues())
        {
            // 输出通知信息到控制台
            Console.WriteLine(
                $@"[通知] {item.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
        }
    }

    /// <summary>
    /// 释放资源方法
    /// 实现IDisposable接口，负责释放使用的资源
    /// </summary>
    public void Dispose()
    {
        // 发出取消请求
        _cancellationTokenSource?.Cancel();
        // 释放取消令牌源资源
        _cancellationTokenSource?.Dispose();
    }
}