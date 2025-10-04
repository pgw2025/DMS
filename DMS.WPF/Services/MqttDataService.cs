using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// MQTT数据服务类，负责管理MQTT服务器相关的数据和操作。
/// </summary>
public class MqttDataService : IMqttDataService
{
    private readonly IMapper _mapper;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IMqttAppService _mqttAppService;


    /// <summary>
    /// MqttDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="mqttAppService">MQTT应用服务实例。</param>
    public MqttDataService(IMapper mapper, IAppDataStorageService appDataStorageService, IDataStorageService dataStorageService, IMqttAppService mqttAppService)
    {
        _mapper = mapper;
        _appDataStorageService = appDataStorageService;
        _dataStorageService = dataStorageService;
        _mqttAppService = mqttAppService;
    }

    /// <summary>
    /// 加载所有MQTT服务器数据。
    /// </summary>
    public async Task LoadMqttServers()
    {
        try
        {
            // 加载MQTT服务器数据
            foreach (var mqttServerDto in _appDataStorageService.MqttServers.Values)
            {
                _dataStorageService.MqttServers.TryAdd(mqttServerDto.Id,_mapper.Map<MqttServerItemViewModel>(mqttServerDto));
            }

        }
        catch (Exception ex)
        {
            // 记录异常或处理错误
            Console.WriteLine($"加载MQTT服务器数据时发生错误: {ex.Message}");
        }
    }


    /// <summary>
    /// 添加MQTT服务器。
    /// </summary>
    public async Task<MqttServerItemViewModel> AddMqttServer(MqttServerItemViewModel mqttServer)
    {
        var dto = _mapper.Map<MqttServerDto>(mqttServer);
        var id = await _mqttAppService.CreateMqttServerAsync(dto);
        dto.Id = id;
        
        var mqttServerItem = _mapper.Map<MqttServerItemViewModel>(dto);
        _dataStorageService.MqttServers.Add(mqttServerItem.Id,mqttServerItem);
        
        return mqttServerItem;
    }

    /// <summary>
    /// 更新MQTT服务器。
    /// </summary>
    public async Task<bool> UpdateMqttServer(MqttServerItemViewModel mqttServer)
    {
        var dto = _mapper.Map<MqttServerDto>(mqttServer);
        await _mqttAppService.UpdateMqttServerAsync(dto);
        return true;
    }

    /// <summary>
    /// 删除MQTT服务器。
    /// </summary>
    public async Task<bool> DeleteMqttServer(MqttServerItemViewModel mqttServer)
    {
        await _mqttAppService.DeleteMqttServerAsync(mqttServer.Id);
        _dataStorageService.MqttServers.Remove(mqttServer.Id);
        return true;
    }
}