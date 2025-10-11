using DMS.Core.Models;

/// <summary>
/// 领域模型：代表一个变量到一个MQTT服务器的特定关联，包含专属别名。
/// 这是一个关联实体，用于解决多对多关系中需要额外属性（别名）的问题。
/// </summary>
public class MqttAlias
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联的变量ID。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 关联的MQTT服务器ID。
    /// </summary>
    public int MqttServerId { get; set; }

    /// <summary>
    /// 针对此特定[变量-服务器]连接的发布别名。此别名将用于构建MQTT Topic。
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// 关联的变量导航属性。
    /// </summary>
    public Variable Variable { get; set; }

    /// <summary>
    /// 关联的MQTT服务器导航属性。
    /// </summary>
    public MqttServer MqttServer { get; set; }

    public MqttAlias()
    {
        
    }

    public MqttAlias(int variableId, int mqttServerId, string alias)
    {
        VariableId = variableId;
        MqttServerId = mqttServerId;
        Alias = alias;
    }
}