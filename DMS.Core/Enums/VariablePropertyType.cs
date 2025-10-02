namespace DMS.Core.Enums
{
    /// <summary>
    /// 变量属性类型枚举
    /// </summary>
    public enum VariablePropertyType
    {
        /// <summary>
        /// 名称
        /// </summary>
        Name,

        /// <summary>
        /// 地址
        /// </summary>
        Address,

        /// <summary>
        /// 数据类型
        /// </summary>
        DataType,

        /// <summary>
        /// 转换公式
        /// </summary>
        ConversionFormula,

        /// <summary>
        /// OPC UA 更新类型
        /// </summary>
        OpcUaUpdateType,

        /// <summary>
        /// MQTT 别名
        /// </summary>
        MqttAlias,

        /// <summary>
        /// 描述
        /// </summary>
        Description,

        /// <summary>
        /// 单位
        /// </summary>
        Unit,

        /// <summary>
        /// 最小值
        /// </summary>
        MinValue,

        /// <summary>
        /// 最大值
        /// </summary>
        MaxValue,

        /// <summary>
        /// 默认值
        /// </summary>
        DefaultValue,

        /// <summary>
        /// 是否激活
        /// </summary>
        IsActive,

        /// <summary>
        /// 访问类型
        /// </summary>
        AccessType,

        /// <summary>
        /// 读写类型
        /// </summary>
        ReadWriteType,

        /// <summary>
        /// 变量表ID
        /// </summary>
        VariableTableId,

        /// <summary>
        /// 值
        /// </summary>
        Value,

        /// <summary>
        /// S7地址
        /// </summary>
        S7Address,

        /// <summary>
        /// OPC UA节点ID
        /// </summary>
        OpcUaNodeId,

        /// <summary>
        /// 轮询间隔
        /// </summary>
        PollingInterval,

        /// <summary>
        /// 信号类型
        /// </summary>
        SignalType,

        /// <summary>
        /// 协议类型
        /// </summary>
        Protocol,

        /// <summary>
        /// 所有属性
        /// </summary>
        All
    }
}