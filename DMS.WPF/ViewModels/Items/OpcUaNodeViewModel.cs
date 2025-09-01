using CommunityToolkit.Mvvm.ComponentModel;
using Opc.Ua;
using System.Collections.ObjectModel;

namespace DMS.WPF.ViewModels.Items
{
    public partial class OpcUaNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private NodeId _nodeId;

        [ObservableProperty]
        private NodeType _nodeType;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isLoaded;

        public ObservableCollection<OpcUaNodeViewModel> Children { get; set; }

        public OpcUaNodeViewModel(string displayName, NodeId nodeId, NodeType nodeType)
        {
            DisplayName = displayName;
            NodeId = nodeId;
            NodeType = nodeType;
            Children = new ObservableCollection<OpcUaNodeViewModel>();
            
            // 如果是文件夹或对象，添加一个虚拟子节点，用于懒加载
            if (nodeType == NodeType.Folder || nodeType == NodeType.Object)
            {
                Children.Add(new OpcUaNodeViewModel("Loading...", NodeId.Null, NodeType.Folder)); // 虚拟节点
            }
        }
    }

    public enum NodeType
    {
        Folder,
        Object,
        Variable
    }
}