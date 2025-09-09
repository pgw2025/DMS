using System.Collections.Concurrent;
using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

public interface IAppDataStorageService
{
    /// <summary>
    /// 安全字典，用于存储所有设备数据
    /// </summary>
    ConcurrentDictionary<int, DeviceDto> Devices { get; }

    /// <summary>
    /// 安全字典，用于存储所有变量表数据
    /// </summary>
    ConcurrentDictionary<int, VariableTableDto> VariableTables { get; }

    /// <summary>
    /// 安全字典，用于存储所有变量数据
    /// </summary>
    ConcurrentDictionary<int, VariableDto> Variables { get; }

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    ConcurrentDictionary<int, MenuBeanDto> Menus { get; }

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    ConcurrentDictionary<int, MenuBeanDto> MenuTrees { get; }

    /// <summary>
    /// 安全字典，用于存储所有MQTT服务器数据
    /// </summary>
    ConcurrentDictionary<int, MqttServerDto> MqttServers { get; }

    /// <summary>
    /// 安全字典，用于存储所有日志数据
    /// </summary>
    ConcurrentDictionary<int, NlogDto> Nlogs { get; }

    /// <summary>
    /// 安全字典，用于存储所有MQTT变量别名的数据
    /// </summary>
    ConcurrentDictionary<int, VariableMqttAliasDto> VariableMqttAliases { get; }
}