using System;
using SqlSugar;

namespace DMS.Data.Entities;

/// <summary>
/// 表示变量数据与MQTT服务器之间的关联实体，包含MQTT别名。
/// </summary>
[SugarTable("VariableMqtt")]
public class DbVariableMqtt
{
    /// <summary>
    /// 关联的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 关联的变量数据ID。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 关联的MQTT服务器ID。
    /// </summary>
    public int MqttId { get; set; }

    /// <summary>
    /// 变量在该MQTT服务器上的别名。
    /// </summary>
    public string MqttAlias { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTime UpdateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 导航属性：关联的变量数据。
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(VariableId))]
    public DbVariable? Variable { get; set; }

    /// <summary>
    /// 导航属性：关联的MQTT服务器。
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(MqttId))]
    public DbMqtt? Mqtt { get; set; }
}
