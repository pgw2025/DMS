using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

[SugarTable("Mqtt")]
public class DbMqtt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增
    public int Id { get; set; }

    /// <summary>
    /// MQTT主机
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// MQTT主机端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Mqtt平台
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MqttPlatform Platform { get; set; }


    /// <summary>
    /// MQTT客户端名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// MQTT客户端登录用户名
    /// </summary>
    public string UserName { get; set; } = String.Empty;

    /// <summary>
    /// MQTT客户端登录密码
    /// </summary>
    public string PassWord { get; set; } = String.Empty;

    /// <summary>
    /// MQTT客户端ID
    /// </summary>
    public string ClientID { get; set; }= String.Empty;

    /// <summary>
    /// MQTT发布主题
    /// </summary>
    public string PublishTopic { get; set; } = String.Empty;

    /// <summary>
    /// MQTT订阅主题
    /// </summary>
    public string SubTopics { get; set; }= String.Empty;

    /// <summary>
    /// 是否设置为默认MQTT客户端
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }= DateTime.Now;

    /// <summary>
    /// MQTT备注
    /// </summary>
    public string Remark { get; set; } = String.Empty;
}