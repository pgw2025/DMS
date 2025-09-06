using DMS.Core.Models;
using DMS.Infrastructure.Interfaces.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// MQTT设备上下文，用于存储单个MQTT服务器的连接信息和状态
    /// </summary>
    public class MqttDeviceContext
    {
        /// <summary>
        /// MQTT服务器配置
        /// </summary>
        public MqttServer MqttServer { get; set; }

        /// <summary>
        /// MQTT服务实例
        /// </summary>
        public IMqttService MqttService { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 重连尝试次数
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// 与该MQTT服务器关联的所有变量MQTT别名
        /// </summary>
        public ConcurrentDictionary<int, VariableMqttAlias> VariableMqttAliases { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MqttDeviceContext()
        {
            VariableMqttAliases = new ConcurrentDictionary<int, VariableMqttAlias>();
            ReconnectAttempts = 0;
        }
    }
}