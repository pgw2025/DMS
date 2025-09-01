using Opc.Ua;

namespace DMS.Infrastructure.Models
{
    /// <summary>
    /// 封装OPC UA节点的基本信息。
    /// </summary>
    public class OpcUaNode
    {
        /// <summary>
        /// 节点的唯一标识符。
        /// </summary>
        public NodeId? NodeId { get; set; }

        /// <summary>
        /// 节点的显示名称。
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 节点的类型（如对象、变量等）。
        /// </summary>
        public NodeClass NodeClass { get; set; }

        /// <summary>
        /// 节点的值。仅当节点是变量类型时有效。
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public OpcUaNode? ParentNode { get; set; }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public List<OpcUaNode> Children { get; set; } = new List<OpcUaNode>();

        /// <summary>
        /// 返回节点的字符串表示形式。
        /// </summary>
        public override string ToString()
        {
            string valueString = Value != null ? $", Value: {Value}" : "";
            return $"- {DisplayName} ({NodeClass}, {NodeId}{valueString})";
        }
    }
}