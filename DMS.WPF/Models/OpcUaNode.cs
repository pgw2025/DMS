using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Opc.Ua;

namespace DMS.WPF.Models;

/// <summary>
/// 表示OPC UA节点，用于构建节点树。
/// </summary>
public partial class OpcUaNode : ObservableObject
{
    /// <summary>
    /// 节点的显示名称。
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 节点的唯一标识符。
    /// </summary>
    public NodeId NodeId { get; set; }

    /// <summary>
    /// 节点的类型（例如，文件夹、变量）。
    /// </summary>
    public NodeType NodeType { get; set; }

    /// <summary>
    /// 子节点集合。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<OpcUaNode> _children;

    /// <summary>
    /// 指示节点是否已加载子节点。
    /// </summary>
    [ObservableProperty]
    private bool _isLoaded;

    /// <summary>
    /// 指示节点是否正在加载子节点。
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 节点的完整路径（可选，用于调试或显示）。
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="displayName">显示名称。</param>
    /// <param name="nodeId">节点ID。</param>
    /// <param name="nodeType">节点类型。</param>
    public OpcUaNode(string displayName, NodeId nodeId, NodeType nodeType)
    {
        DisplayName = displayName;
        NodeId = nodeId;
        NodeType = nodeType;
        Children = new ObservableCollection<OpcUaNode>();
    }
}

/// <summary>
/// OPC UA节点类型枚举。
/// </summary>
public enum NodeType
{
    Folder,
    Object,
    Variable
}
