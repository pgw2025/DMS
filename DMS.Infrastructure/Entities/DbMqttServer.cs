using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// MQTT服务器配置实体
/// </summary>
public class DbMqttServer
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// MQTT服务器URL
    /// </summary>
    public string ServerUrl { get; set; }

    /// <summary>
    /// 端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 订阅的主题
    /// </summary>
    public string SubscribeTopic { get; set; }

    /// <summary>
    /// 发布的主题
    /// </summary>
    public string PublishTopic { get; set; }

    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// 连接持续时间（秒）
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long ConnectionDuration { get; set; }

    /// <summary>
    /// 消息格式
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string MessageFormat { get; set; }
}