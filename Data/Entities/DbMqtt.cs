using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的MQTT配置实体。
/// </summary>
[SugarTable("Mqtt")]
public class DbMqtt
{
    /// <summary>
    /// MQTT客户端ID。
    /// </summary>
    public string ClientID { get; set; } = String.Empty;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// MQTT主机。
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// MQTT配置的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增
    public int Id { get; set; }

    /// <summary>
    /// 是否启用此MQTT配置。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 是否设置为默认MQTT客户端。
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// MQTT客户端名字。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// MQTT客户端登录密码。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string PassWord { get; set; } = String.Empty;

    /// <summary>
    /// Mqtt平台类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MqttPlatform Platform { get; set; }

    /// <summary>
    /// MQTT主机端口。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// MQTT发布主题。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string PublishTopic { get; set; } = String.Empty;

    /// <summary>
    /// MQTT备注。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Remark { get; set; } = String.Empty;

    /// <summary>
    /// MQTT订阅主题。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string SubTopic { get; set; } = String.Empty;

    /// <summary>
    /// MQTT客户端登录用户名。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string UserName { get; set; } = String.Empty;

    /// <summary>
    /// 关联的变量数据列表。
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DbVariableMqtt.MqttId))]
    public List<DbVariableMqtt>? VariableMqtts { get; set; }
}