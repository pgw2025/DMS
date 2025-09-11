using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

public class AppDataStorageService : IAppDataStorageService
{
    /// <summary>
    /// 安全字典，用于存储所有设备数据
    /// </summary>
    public ConcurrentDictionary<int, DeviceDto> Devices { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有变量表数据
    /// </summary>
    public ConcurrentDictionary<int, VariableTableDto> VariableTables { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有变量数据
    /// </summary>
    public ConcurrentDictionary<int, VariableDto> Variables { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    public ConcurrentDictionary<int, MenuBeanDto> Menus { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有菜单数据
    /// </summary>
    public ConcurrentDictionary<int, MenuBeanDto> MenuTrees { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有MQTT服务器数据
    /// </summary>
    public ConcurrentDictionary<int, MqttServerDto> MqttServers { get; } = new();
    
    
    /// <summary>
    /// 安全字典，用于存储所有MQTT变量别名的数据
    /// </summary>
    public ConcurrentDictionary<int, VariableMqttAliasDto> VariableMqttAliases { get; } = new();
    
        
    
    /// <summary>
    /// 安全字典，用于存储所有历史记录
    /// </summary>
    public ConcurrentDictionary<int, VariableHistoryDto> VariableHistories { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有日志数据
    /// </summary>
    public ConcurrentDictionary<int, NlogDto> Nlogs { get; } = new();
}