using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义数据管理相关的应用服务操作，负责管理所有的数据，包括设备、变量表和变量。
/// </summary>
public interface IAppDataCenterService
{
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

    #region 菜单管理

    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    Task<List<MenuBeanDto>> GetAllMenusAsync();

    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    Task<MenuBeanDto> GetMenuByIdAsync(int id);

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    Task<int> CreateMenuAsync(MenuBeanDto menuDto);

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    Task UpdateMenuAsync(MenuBeanDto menuDto);

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    Task DeleteMenuAsync(int id);

    /// <summary>
    /// 在内存中添加菜单
    /// </summary>
    void AddMenuToMemory(MenuBeanDto menuDto);

    /// <summary>
    /// 在内存中更新菜单
    /// </summary>
    void UpdateMenuInMemory(MenuBeanDto menuDto);

    /// <summary>
    /// 在内存中删除菜单
    /// </summary>
    void RemoveMenuFromMemory(int menuId);

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    List<MenuBeanDto> GetRootMenus();

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    List<MenuBeanDto> GetChildMenus(int parentId);

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
    /// 异步创建一个新变量。
    /// </summary>
    Task<VariableDto> CreateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    Task<int> UpdateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos);

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    Task<bool> DeleteVariableAsync(int id);

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    Task<bool> DeleteVariablesAsync(List<int> ids);

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

    #endregion

    #region MQTT服务器管理

    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    Task<MqttServerDto> GetMqttServerByIdAsync(int id);

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    Task<List<MqttServerDto>> GetAllMqttServersAsync();

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    Task UpdateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    Task DeleteMqttServerAsync(int id);

    /// <summary>
    /// 在内存中添加MQTT服务器
    /// </summary>
    void AddMqttServerToMemory(MqttServerDto mqttServerDto);

    /// <summary>
    /// 在内存中更新MQTT服务器
    /// </summary>
    void UpdateMqttServerInMemory(MqttServerDto mqttServerDto);

    /// <summary>
    /// 在内存中删除MQTT服务器
    /// </summary>
    void RemoveMqttServerFromMemory(int mqttServerId);

    #endregion

    #region 日志管理

    /// <summary>
    /// 异步根据ID获取日志DTO。
    /// </summary>
    Task<NlogDto> GetNlogByIdAsync(int id);

    /// <summary>
    /// 异步获取所有日志DTO列表。
    /// </summary>
    Task<List<NlogDto>> GetAllNlogsAsync();

    /// <summary>
    /// 异步获取指定数量的最新日志DTO列表。
    /// </summary>
    Task<List<NlogDto>> GetLatestNlogsAsync(int count);

    /// <summary>
    /// 异步清空所有日志。
    /// </summary>
    Task ClearAllNlogsAsync();

    /// <summary>
    /// 在内存中添加日志
    /// </summary>
    void AddNlogToMemory(NlogDto nlogDto);

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    void UpdateNlogInMemory(NlogDto nlogDto);

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    void RemoveNlogFromMemory(int nlogId);

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

    /// <summary>
    /// 获取所有菜单的安全字典。
    /// </summary>
    ConcurrentDictionary<int, MenuBeanDto> Menus { get; }
    /// <summary>
    /// 获取所有菜单树的安全字典。
    /// </summary>
    ConcurrentDictionary<int, MenuBeanDto> MenuTrees { get; }

    /// <summary>
    /// 获取所有MQTT服务器的安全字典。
    /// </summary>
    ConcurrentDictionary<int, MqttServerDto> MqttServers { get; }
    
    /// <summary>
    /// 获取所有日志的安全字典。
    /// </summary>
    ConcurrentDictionary<int, NlogDto> Nlogs { get; }

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

    /// <summary>
    /// 异步加载所有菜单数据。
    /// </summary>
    Task<List<MenuBeanDto>> LoadAllMenusAsync();

    /// <summary>
    /// 异步加载所有MQTT服务器数据。
    /// </summary>
    Task<List<MqttServerDto>> LoadAllMqttServersAsync();
    
    /// <summary>
    /// 异步加载所有日志数据。
    /// </summary>
    Task<List<NlogDto>> LoadAllNlogsAsync();

    #endregion

    #region 事件定义

    /// <summary>
    /// 当数据加载完成时触发
    /// </summary>
    event EventHandler<DataLoadCompletedEventArgs> OnLoadDataCompleted;

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
    /// 当菜单数据发生变化时触发
    /// </summary>
    event EventHandler<MenuChangedEventArgs> MenuChanged;

    /// <summary>
    /// 当MQTT服务器数据发生变化时触发
    /// </summary>
    event EventHandler<MqttServerChangedEventArgs> MqttServerChanged;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    event EventHandler<NlogChangedEventArgs> NlogChanged;

    /// <summary>
    /// 当变量值发生变化时触发
    /// </summary>
    event EventHandler<VariableValueChangedEventArgs> VariableValueChanged;

    void OnVariableValueChanged(VariableValueChangedEventArgs e);


    #endregion
}