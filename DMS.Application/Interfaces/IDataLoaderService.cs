using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义数据加载服务接口，负责从数据源加载数据到内存中
/// </summary>
public interface IDataLoaderService
{
    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中
    /// </summary>
    Task LoadAllDataToMemoryAsync();

    /// <summary>
    /// 异步加载所有设备数据
    /// </summary>
    Task<List<DeviceDto>> LoadAllDevicesAsync();

    /// <summary>
    /// 异步加载所有变量表数据
    /// </summary>
    Task<List<VariableTableDto>> LoadAllVariableTablesAsync();

    /// <summary>
    /// 异步加载所有变量数据
    /// </summary>
    Task<List<VariableDto>> LoadAllVariablesAsync();

    /// <summary>
    /// 异步加载所有菜单数据
    /// </summary>
    Task<List<MenuBeanDto>> LoadAllMenusAsync();

    /// <summary>
    /// 异步加载所有MQTT服务器数据
    /// </summary>
    Task<List<MqttServerDto>> LoadAllMqttServersAsync();

    /// <summary>
    /// 异步加载所有日志数据
    /// </summary>
    Task<List<NlogDto>> LoadAllNlogsAsync(int count);

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    event EventHandler<DataLoadCompletedEventArgs> OnLoadDataCompleted;
}