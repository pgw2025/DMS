using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义数据管理相关的应用服务操作，负责管理所有的数据，包括设备、变量表和变量。
/// </summary>
public interface IDataCenterService
{
    #region 事件定义

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    event EventHandler<DataLoadCompletedEventArgs> DataLoadCompleted;

    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    event EventHandler<DeviceChangedEventArgs> DeviceChanged;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    event EventHandler<VariableTableChangedEventArgs> VariableTableChanged;

    /// <summary>
    /// 当变量数据发生变化时触发
    /// </summary>
    event EventHandler<VariableChangedEventArgs> VariableChanged;

    /// <summary>
    /// 当数据发生任何变化时触发
    /// </summary>
    event EventHandler<DataChangedEventArgs> DataChanged;

    #endregion
    #region 设备管理

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    Task<DeviceDto> GetDeviceByIdAsync(int id);

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    Task<List<DeviceDto>> GetAllDevicesAsync();

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    /// <param name="dto">包含设备、变量表和菜单信息的DTO。</param>
    /// <returns>新创建设备的DTO。</returns>
    Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    Task<int> UpdateDeviceAsync(DeviceDto deviceDto);

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    Task<bool> DeleteDeviceByIdAsync(int deviceId);

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    Task ToggleDeviceActiveStateAsync(int id);

    /// <summary>
    /// 在内存中添加设备
    /// </summary>
    void AddDeviceToMemory(DeviceDto deviceDto);

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    void UpdateDeviceInMemory(DeviceDto deviceDto);

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    void RemoveDeviceFromMemory(int deviceId);

    #endregion

    #region 变量表管理

    /// <summary>
    /// 异步根据ID获取变量表DTO。
    /// </summary>
    Task<VariableTableDto> GetVariableTableByIdAsync(int id);

    /// <summary>
    /// 异步获取所有变量表DTO列表。
    /// </summary>
    Task<List<VariableTableDto>> GetAllVariableTablesAsync();

    /// <summary>
    /// 异步创建一个新变量表及其关联菜单（事务性操作）。
    /// </summary>
    /// <param name="dto">包含变量表和菜单信息的DTO。</param>
    /// <returns>新创建变量表的DTO。</returns>
    Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto dto);

    /// <summary>
    /// 异步更新一个已存在的变量表。
    /// </summary>
    Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto);

    /// <summary>
    /// 异步删除一个变量表。
    /// </summary>
    Task<bool> DeleteVariableTableAsync(int id);

    /// <summary>
    /// 在内存中添加变量表
    /// </summary>
    void AddVariableTableToMemory(VariableTableDto variableTableDto);

    /// <summary>
    /// 在内存中更新变量表
    /// </summary>
    void UpdateVariableTableInMemory(VariableTableDto variableTableDto);

    /// <summary>
    /// 在内存中删除变量表
    /// </summary>
    void RemoveVariableTableFromMemory(int variableTableId);

    #endregion

    #region 变量管理

    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    Task<VariableDto> GetVariableByIdAsync(int id);

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    Task<List<VariableDto>> GetAllVariablesAsync();

    /// <summary>
    /// 异步创建一个新变量（事务性操作）。
    /// </summary>
    Task<VariableDto> CreateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步更新一个已存在的变量（事务性操作）。
    /// </summary>
    Task<int> UpdateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步批量更新变量（事务性操作）。
    /// </summary>
    Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos);

    /// <summary>
    /// 异步删除一个变量（事务性操作）。
    /// </summary>
    Task<bool> DeleteVariableAsync(int id);

    /// <summary>
    /// 异步批量删除变量（事务性操作）。
    /// </summary>
    Task<bool> DeleteVariablesAsync(List<int> ids);

    /// <summary>
    /// 异步批量导入变量。
    /// </summary>
    Task<bool> BatchImportVariablesAsync(List<VariableDto> variables);

    /// <summary>
    /// 检测一组变量是否已存在。
    /// </summary>
    /// <param name="variablesToCheck">要检查的变量列表。</param>
    /// <returns>返回输入列表中已存在的变量。</returns>
    Task<List<VariableDto>> FindExistingVariablesAsync(IEnumerable<VariableDto> variablesToCheck);

    /// <summary>
    /// 检测单个变量是否已存在。
    /// </summary>
    /// <param name="variableToCheck">要检查的变量。</param>
    /// <returns>如果变量已存在则返回该变量，否则返回null。</returns>
    Task<VariableDto?> FindExistingVariableAsync(VariableDto variableToCheck);

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    void AddVariableToMemory(VariableDto variableDto);

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    void UpdateVariableInMemory(VariableDto variableDto);

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    void RemoveVariableFromMemory(int variableId);

    /// <summary>
    /// 批量在内存中添加变量
    /// </summary>
    void AddVariablesToMemory(List<VariableDto> variables);

    /// <summary>
    /// 批量在内存中更新变量
    /// </summary>
    void UpdateVariablesInMemory(List<VariableDto> variables);

    /// <summary>
    /// 批量在内存中删除变量
    /// </summary>
    void RemoveVariablesFromMemory(List<int> variableIds);

    #endregion

    #region 数据存储访问

    /// <summary>
    /// 获取所有设备的安全字典。
    /// </summary>
    ConcurrentDictionary<int, DeviceDto> Devices { get; }

    /// <summary>
    /// 获取所有变量表的安全字典。
    /// </summary>
    ConcurrentDictionary<int, VariableTableDto> VariableTables { get; }

    /// <summary>
    /// 获取所有变量的安全字典。
    /// </summary>
    ConcurrentDictionary<int, VariableDto> Variables { get; }

    #endregion

    #region 数据加载和初始化

    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中。
    /// </summary>
    Task LoadAllDataToMemoryAsync();

    /// <summary>
    /// 异步加载所有设备及其关联数据。
    /// </summary>
    Task<List<DeviceDto>> LoadAllDevicesAsync();

    /// <summary>
    /// 异步加载所有变量表及其关联数据。
    /// </summary>
    Task<List<VariableTableDto>> LoadAllVariableTablesAsync();

    /// <summary>
    /// 异步加载所有变量数据。
    /// </summary>
    Task<List<VariableDto>> LoadAllVariablesAsync();

    #endregion
}