using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Enums;
using DMS.Data.Entities;

namespace DMS.Models;

/// <summary>
/// 表示变量数据与MQTT服务器之间的关联模型，包含MQTT别名。
/// </summary>
public partial class VariableMqtt : ObservableObject
{
    public VariableMqtt()
    {
    }

    public VariableMqtt(Variable variable, Mqtt mqtt)
    {
        if (mqtt != null && variable != null)
        {
             Variable = variable;
                    Mqtt = mqtt;
                    MqttAlias = MqttAlias != String.Empty ? MqttAlias : variable.Name;
        }
       
    }

    /// <summary>
    /// 关联的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联的变量数据ID。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 关联的MQTT服务器ID。
    /// </summary>
    public int MqttId { get; set; }

    /// <summary>
    /// 变量在该MQTT服务器上的别名。
    /// </summary>
    [ObservableProperty]
    private string _mqttAlias = string.Empty;

    /// <summary>
    /// 变量的唯一标识符（S7地址或OPC UA NodeId）。
    /// </summary>
    public string Identifier
    {
        get
        {
            if (Variable!=null)
            {
                
            
            if (Variable.ProtocolType == ProtocolType.S7)
            {
                return Variable.S7Address;
            }
            else if (Variable.ProtocolType == ProtocolType.OpcUA)
            {
                return Variable.OpcUaNodeId;
            }
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTime UpdateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 导航属性：关联的变量数据。
    /// </summary>
    public Variable Variable { get; set; }

    /// <summary>
    /// 导航属性：关联的MQTT服务器。
    /// </summary>
    public Mqtt Mqtt { get; set; }
}