using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Models;

namespace DMS.Application.Services;

public class AppDataStorageService : IAppDataStorageService
{
    /// <summary>
    /// 安全字典，用于存储所有设备数据
    /// </summary>
            public ConcurrentDictionary<int, Device> Devices { get; } = new();
    /// <summary>
    /// 安全字典，用于存储所有变量表数据
    /// </summary>
    public ConcurrentDictionary<int, VariableTable> VariableTables { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有变量数据
    /// </summary>
    public ConcurrentDictionary<int, Variable> Variables { get; } = new();

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
    public ConcurrentDictionary<int, MqttServer> MqttServers { get; } = new();
    
    
    /// <summary>
    /// 安全字典，用于存储所有MQTT变量别名的数据
    /// </summary>
    public ConcurrentDictionary<int, MqttAlias> MqttAliases { get; } = new();
    
        
    
    /// <summary>
    /// 安全字典，用于存储所有历史记录
    /// </summary>
    public ConcurrentDictionary<int, VariableHistoryDto> VariableHistories { get; } = new();

    /// <summary>
    /// 安全字典，用于存储所有日志数据
    /// </summary>
    public ConcurrentDictionary<int, NlogDto> Nlogs { get; } = new();
    
    /// <summary>
    /// 安全字典，用于存储所有触发器定义数据
    /// </summary>
    public ConcurrentDictionary<int, TriggerDefinitionDto> Triggers { get; } = new();
}