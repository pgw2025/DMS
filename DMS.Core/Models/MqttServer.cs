namespace DMS.Core.Models;

/// <summary>
/// 代表一个MQTT Broker的配置。
/// </summary>
public class MqttServer
{
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string ServerUrl { get; set; } // Broker地址
    public int Port { get; set; } // 端口
    public string Username { get; set; } // 用户名
    public string Password { get; set; } // 密码
    public bool IsActive { get; set; } // 是否启用
    public bool IsConnect { get; set; } // 是否启用


    /// <summary>
    /// MQTT订阅主题。
    /// </summary>
    public string SubscribeTopic { get; set; }

    /// <summary>
    /// MQTT发布主题。
    /// </summary>
    public string PublishTopic { get; set; }

    /// <summary>
    /// MQTT客户端ID。
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// MQTT服务器配置的创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// MQTT客户端连接到Broker的时间。
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// MQTT客户端连接时长（秒）。
    /// </summary>
    public long ConnectionDuration { get; set; }

    /// <summary>
    /// 报文格式，例如JSON, PlainText等。
    /// </summary>
    public string MessageFormat { get; set; }

    /// <summary>
    /// 消息头格式。
    /// </summary>
    public string MessageHeader { get; set; }

    /// <summary>
    /// 消息内容格式。
    /// </summary>
    public string MessageContent { get; set; }

    /// <summary>
    /// 消息尾格式。
    /// </summary>
    public string MessageFooter { get; set; }

    /// <summary>
    /// 与此服务器关联的所有变量别名。通过此集合可以反向查找关联的变量。
    /// </summary>
    public List<VariableMqttAlias> VariableAliases { get; set; } = new();
}