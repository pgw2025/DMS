using PMSWPF.Enums;

namespace PMSWPF.Models;

public class Mqtt
{
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
    public MqttPlatform Platform { get; set; }


    /// <summary>
    /// MQTT客户端名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// MQTT客户端登录用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// MQTT客户端登录密码
    /// </summary>
    public string PassWord { get; set; } //变量状态 

    /// <summary>
    /// MQTT客户端ID
    /// </summary>
    public string ClientID { get; set; }

    /// <summary>
    /// MQTT发布主题
    /// </summary>
    public string PublishTopic { get; set; }

    /// <summary>
    /// MQTT订阅主题
    /// </summary>
    public string SubTopics { get; set; }

    /// <summary>
    /// 是否设置为默认MQTT客户端
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnTime { get; set; }

    /// <summary>
    /// MQTT备注
    /// </summary>
    public string Remark { get; set; } = String.Empty;
}