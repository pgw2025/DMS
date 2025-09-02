using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Infrastructure.Models;
using Opc.Ua;
using System.Collections.ObjectModel;

namespace DMS.WPF.ViewModels.Items
{
    /// <summary>
    /// OPC UA节点的视图模型。
    /// </summary>
    public partial class OpcUaNodeItemViewModel : ObservableObject
    {

        [ObservableProperty]
        private NodeId? _nodeId;

        [ObservableProperty]
        private string? _displayName;

        [ObservableProperty]
        private NodeClass _nodeClass;

        [ObservableProperty]
        private object? _value;

        [ObservableProperty]
        private string? _dataType;

        [ObservableProperty]
        private OpcUaNodeItemViewModel? _parentNode;

        [ObservableProperty]
        private ObservableCollection<OpcUaNodeItemViewModel> _children = new ObservableCollection<OpcUaNodeItemViewModel>();

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// 默认构造函数（用于设计时支持）。
        /// </summary>
        public OpcUaNodeItemViewModel()
        {
            // 设计时数据支持
           
        }
        
      
    }
}