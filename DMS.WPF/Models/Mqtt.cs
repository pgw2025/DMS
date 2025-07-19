using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Enums;

namespace DMS.WPF.Models;

/// <summary>
/// 表示MQTT配置信息。
/// </summary>
public partial class Mqtt : ObservableObject
{
    public event Action<Mqtt> OnMqttIsActiveChanged; 
    
    /// <summary>
    /// MQTT客户端ID。
    /// </summary>
    public string ClientID { get; set; }

    /// <summary>
    /// 连接时间。
    /// </summary>
    public DateTime ConnTime { get; set; }

    /// <summary>
    /// MQTT主机。
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// MQTT配置的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 是否启用此MQTT配置。
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    partial void OnIsActiveChanged(bool value)
    {
        OnMqttIsActiveChanged?.Invoke(this);
    }

    /// <summary>
    /// 显示连接的消息：
    /// </summary>
    [ObservableProperty]
    private string connectMessage;

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
    public string PassWord { get; set; }

    /// <summary>
    /// Mqtt平台类型。
    /// </summary>
    public MqttPlatform Platform { get; set; }

    /// <summary>
    /// MQTT主机端口。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// MQTT发布主题。
    /// </summary>
    public string PublishTopic { get; set; }

    /// <summary>
    /// MQTT备注。
    /// </summary>
    public string Remark { get; set; } = String.Empty;

    /// <summary>
    /// MQTT订阅主题。
    /// </summary>
    public string SubTopic { get; set; }

    /// <summary>
    /// MQTT客户端登录用户名。
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// 关联的变量数据列表。
    /// </summary>
    public List<VariableMqtt>? VariableMqtts { get; set; }

    /// <summary>
    /// 是否连接。
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;
}