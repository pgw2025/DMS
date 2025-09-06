using DMS.Core.Models;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示和管理变量与MQTT服务器关联别名的DTO。
/// </summary>
public class VariableMqttAliasDto
{
    public int Id { get; set; }
    public int VariableId { get; set; }
    public int MqttServerId { get; set; }
    public string MqttServerName { get; set; } // 用于UI显示关联的服务器名称
    public string Alias { get; set; }
    
    /// <summary>
    /// 关联的变量对象。
    /// </summary>
    public Variable Variable { get; set; }
    
    /// <summary>
    /// 关联的MQTT服务器对象。
    /// </summary>
    public MqttServer MqttServer { get; set; }
}