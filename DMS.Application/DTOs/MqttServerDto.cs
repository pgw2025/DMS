using System;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示MQTT服务器配置信息的DTO。
/// </summary>
public class MqttServerDto
{
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string BrokerAddress { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public string SubscribeTopic { get; set; }
    public string PublishTopic { get; set; }
    public string ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public long ConnectionDuration { get; set; }
    public string MessageFormat { get; set; }
}