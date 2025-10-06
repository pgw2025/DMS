using System;
using System.Collections.Generic;
using DMS.Core.Models;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示MQTT服务器配置信息的DTO。
/// </summary>
public class MqttServerDto
{
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string ServerUrl { get; set; }
    public int Port { get; set; }
    public bool IsConnect { get; set; }
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
    public string MessageHeader { get; set; }
    public string MessageContent { get; set; }
    public string MessageFooter { get; set; }
    public List<VariableMqttAlias> VariableAliases { get; set; } = new();
}