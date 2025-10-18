using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Services;

/// <summary>
/// MQTT别名数据服务类，负责管理MQTT别名相关的数据和操作。
/// </summary>
public class MqttAliasDataService : IMqttAliasDataService
{
    private readonly IMapper _mapper;
    private readonly IAppStorageService _appStorageService;
    private readonly IMqttAliasManagementService _mqttAliasManagementService;
    private readonly IDataStorageService _dataStorageService;

    /// <summary>
    /// MqttAliasDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appStorageService">应用数据存储服务实例。</param>
    /// <param name="mqttAliasManagementService">MQTT别名管理服务实例。</param>
    /// <param name="dataStorageService">数据存储服务实例。</param>
    public MqttAliasDataService(IMapper mapper, 
        IAppStorageService appStorageService, 
        IMqttAliasManagementService mqttAliasManagementService, 
        IDataStorageService dataStorageService)
    {
        _mapper = mapper;
        _appStorageService = appStorageService;
        _mqttAliasManagementService = mqttAliasManagementService;
        _dataStorageService = dataStorageService;
    }

    /// <summary>
    /// 加载所有MQTT别名数据。
    /// </summary>
    public async Task LoadMqttAliases()
    {
        try
        {
            // 清空现有数据
            _dataStorageService.MqttAliases.Clear();
            
            // 加载MQTT别名数据
            foreach (var mqttAlias in _appStorageService.MqttAliases.Values)
            {
                MqttAliasItem mqttAliasItem = _mapper.Map<MqttAliasItem>(mqttAlias);
                if(_dataStorageService.MqttAliases.TryAdd(mqttAlias.Id, mqttAliasItem))
                {

                    if (_dataStorageService.MqttServers.TryGetValue(mqttAlias.MqttServerId,out var mqttServerItem))
                    {
                        mqttServerItem.VariableAliases.Add(mqttAliasItem);
                        mqttAliasItem.MqttServer = mqttServerItem;
                    }

                    if (_dataStorageService.Variables.TryGetValue(mqttAlias.VariableId,out var variableItem))
                    {
                        variableItem.MqttAliases.Add(mqttAliasItem);
                        mqttAliasItem.Variable = variableItem;
                    }

                }

            }
        }
        catch (Exception ex)
        {
            // 记录异常或处理错误
            Console.WriteLine($"加载MQTT别名数据时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加MQTT别名。
    /// </summary>
    public async Task<MqttAliasItem> AssignAliasAsync(MqttAliasItem mqttAlias)
    {
        var addMqttAlias = await _mqttAliasManagementService.AssignAliasAsync(_mapper.Map<MqttAlias>(mqttAlias));

       
        if (_dataStorageService.MqttAliases.ContainsKey(addMqttAlias.Id))
        {
            return null;
        }
        mqttAlias.Id = addMqttAlias.Id;

        _dataStorageService.MqttAliases.Add(mqttAlias.Id, mqttAlias);
        if (_dataStorageService.MqttServers.TryGetValue(mqttAlias.MqttServerId, out var mqttServerItem))
        {
            mqttAlias.MqttServer = mqttServerItem;
            mqttServerItem.VariableAliases.Add(mqttAlias);
        }
        if (_dataStorageService.Variables.TryGetValue(mqttAlias.VariableId, out var variableItem))
        {
            mqttAlias.Variable = variableItem;
            variableItem.MqttAliases.Add(mqttAlias);
        }

        return mqttAlias;
    }

    /// <summary>
    /// 更新MQTT别名。
    /// </summary>
    public async Task<bool> UpdateMqttAlias(MqttAliasItem mqttAlias)
    {
        var result = await _mqttAliasManagementService.UpdateAsync(_mapper.Map<MqttAlias>(mqttAlias));

        if (result > 0)
        {
            // 更新界面数据
            if (_dataStorageService.MqttAliases.TryGetValue(mqttAlias.Id, out var existingAlias))
            {
                // 更新现有别名的属性
                existingAlias.VariableId = mqttAlias.VariableId;
                existingAlias.MqttServerId = mqttAlias.MqttServerId;
                existingAlias.Alias = mqttAlias.Alias;
                existingAlias.MqttServerName = mqttAlias.MqttServerName;
            }
        }

        return result > 0;
    }

    /// <summary>
    /// 删除MQTT别名。
    /// </summary>
    public async Task<bool> DeleteMqttAlias(MqttAliasItem mqttAlias)
    {
        // 从数据库和内存中删除MQTT别名
        var result = await _mqttAliasManagementService.DeleteAsync(mqttAlias.Id);

        if (result )
        {
            
            if (_dataStorageService.MqttServers.TryGetValue(mqttAlias.MqttServerId, out var mqttServerItem))
            {
                mqttServerItem.VariableAliases.Remove(mqttAlias);
            }
            if (_dataStorageService.Variables.TryGetValue(mqttAlias.VariableId, out var variableItem))
            {
                variableItem.MqttAliases.Remove(mqttAlias);
            }
            _dataStorageService.MqttAliases.Remove(mqttAlias.Id, out _);
        }

        return result;
    }
    
    /// <summary>
    /// 根据ID获取MQTT别名。
    /// </summary>
    public async Task<MqttAliasItem> GetMqttAliasById(int id)
    {
        if (_dataStorageService.MqttAliases.TryGetValue(id, out var mqttAliasItem))
        {
            return mqttAliasItem;
        }

        // 如果内存中没有，则从数据库查询
        var mqttAlias = await _mqttAliasManagementService.AssignAliasAsync(new MqttAlias { Id = id });
        if (mqttAlias != null)
        {
            var mappedItem = _mapper.Map<MqttAliasItem>(mqttAlias);
            _dataStorageService.MqttAliases.TryAdd(mappedItem.Id, mappedItem);
            return mappedItem;
        }

        return null;
    }
}