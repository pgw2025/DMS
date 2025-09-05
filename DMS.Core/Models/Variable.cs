using DMS.Core.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Core.Models;

/// <summary>
/// 核心数据点，代表从设备读取的单个值。
/// </summary>
public class Variable
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 变量名。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 变量的描述信息。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 在设备中的地址 (例如: DB1.DBD0, M100.0)。
    /// </summary>
    public string S7Address { get; set; }

    /// <summary>
    /// 变量的信号类型，例如启动信号、停止信号。
    /// </summary>
    public SignalType SignalType { get; set; }

    /// <summary>
    /// 变量的轮询间隔（毫秒），决定了其读取频率。
    /// </summary>
    public int PollingInterval { get; set; }

    /// <summary>
    /// 指示此变量是否处于激活状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 所属变量表的ID。
    /// </summary>
    public int VariableTableId { get; set; }

    /// <summary>
    /// 所属变量表的导航属性。
    /// </summary>
    public VariableTable VariableTable { get; set; }

    /// <summary>
    /// 此变量的所有MQTT发布别名关联。一个变量可以关联多个MQTT服务器，每个关联可以有独立的别名。
    /// </summary>
    public List<VariableMqttAlias> MqttAliases { get; set; } = new();

    /// <summary>
    /// OPC UA NodeId (仅当 Protocol 为 OpcUa 时有效)。
    /// </summary>
    public string OpcUaNodeId { get; set; }

    /// <summary>
    /// 是否启用历史数据保存。
    /// </summary>
    public bool IsHistoryEnabled { get; set; }

    /// <summary>
    /// 历史数据保存的死区值。当变量值变化超过此死区时才保存。
    /// </summary>
    public double HistoryDeadband { get; set; }

    /// <summary>
    /// 是否启用报警。
    /// </summary>
    public bool IsAlarmEnabled { get; set; }

    /// <summary>
    /// 报警最小值。
    /// </summary>
    public double AlarmMinValue { get; set; }

    /// <summary>
    /// 报警最大值。
    /// </summary>
    public double AlarmMaxValue { get; set; }

    /// <summary>
    /// 报警死区。当变量值变化超过此死区时才触发报警。
    /// </summary>
    public double AlarmDeadband { get; set; }

    /// <summary>
    /// 存储从设备读取到的最新值。此属性不应持久化到数据库，仅用于运行时。
    /// </summary>
    public string DataValue { get; set; }

    /// <summary>
    /// 变量的通讯协议。
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// 变量的数据类型。
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// 数值转换公式，例如 "+3*5"。
    /// </summary>
    public string ConversionFormula { get; set; }

    /// <summary>
    /// 经过转换公式计算后的显示值。此属性不应持久化到数据库，仅用于运行时。
    /// </summary>
    public string DisplayValue { get; set; }

    /// <summary>
    /// 变量的创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 变量的最后更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最后更新变量的用户。
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// 指示变量是否被修改。
    /// </summary>
    public bool IsModified { get; set; }


    public OpcUaUpdateType OpcUaUpdateType { get; set; }
}