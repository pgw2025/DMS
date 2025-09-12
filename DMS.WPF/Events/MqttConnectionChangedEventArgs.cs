using System;

namespace DMS.WPF.Events;

/// <summary>
/// MQTT连接状态改变事件参数
/// </summary>
public class MqttConnectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// MQTT服务器ID
    /// </summary>
    public int MqttServerId { get; }

    /// <summary>
    /// MQTT服务器名称
    /// </summary>
    public string MqttServerName { get; }

    /// <summary>
    /// 旧连接状态
    /// </summary>
    public bool OldConnectionStatus { get; }

    /// <summary>
    /// 新连接状态
    /// </summary>
    public bool NewConnectionStatus { get; }

    /// <summary>
    /// 状态改变时间
    /// </summary>
    public DateTime ChangeTime { get; }

    /// <summary>
    /// 初始化MqttConnectionChangedEventArgs类的新实例
    /// </summary>
    /// <param name="mqttServerId">MQTT服务器ID</param>
    /// <param name="mqttServerName">MQTT服务器名称</param>
    /// <param name="oldStatus">旧连接状态</param>
    /// <param name="newStatus">新连接状态</param>
    public MqttConnectionChangedEventArgs(int mqttServerId, string mqttServerName, bool oldStatus, bool newStatus)
    {
        MqttServerId = mqttServerId;
        MqttServerName = mqttServerName;
        OldConnectionStatus = oldStatus;
        NewConnectionStatus = newStatus;
        ChangeTime = DateTime.Now;
    }
}