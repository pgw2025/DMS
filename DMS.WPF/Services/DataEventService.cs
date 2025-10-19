using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Core.Models;
using DMS.Message;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using Microsoft.Extensions.Logging;

namespace DMS.WPF.Services;

/// <summary>
/// 数据事件服务类，负责处理数据变更事件和消息传递。
/// </summary>
public class DataEventService : IDataEventService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly IAppCenterService _appCenterService;
    private readonly IWPFDataService _wpfDataService;
    private readonly ILogger<DataEventService> _logger;

    /// <summary>
    /// DataEventService类的构造函数。
    /// </summary>
    public DataEventService(IMapper mapper,
        IDataStorageService dataStorageService, 
        IEventService eventService,
        INotificationService notificationService,
        IAppCenterService appCenterService,
        IWPFDataService wpfDataService,
        ILogger<DataEventService> logger)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _eventService = eventService;
        _notificationService = notificationService;
        _appCenterService = appCenterService;
        _wpfDataService = wpfDataService;
        _logger = logger;
        
        _logger?.LogInformation("正在初始化 DataEventService");
        
        // 监听变量值变更事件
        _eventService.OnVariableValueChanged += OnVariableValueChanged;
        _eventService.OnMqttServerChanged += OnMqttServerChanged;
        _eventService.OnLoadDataCompleted += OnLoadDataCompleted;
        // 监听日志变更事件
        // _appCenterService.OnLogChanged += _logDataService.OnNlogChanged;
        
        _logger?.LogInformation("DataEventService 初始化完成");
    }

    private void OnMqttServerChanged(object? sender, MqttServerChangedEventArgs e)
    {
        _logger?.LogDebug("接收到Mqtt服务器状态发生了改变，服务器名称：{mqttName}属性: {mqttProperty}",
            e.MqttServer.ServerName, e.PropertyType);

        // 在UI线程上更新变量值
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            //// 查找并更新对应的变量
            if (_dataStorageService.MqttServers.TryGetValue(e.MqttServer.Id, out var mqttServerItem))
            {
                if (e.ChangeType == ActionChangeType.Updated)
                {
                    switch (e.PropertyType)
                    {
                        case MqttServerPropertyType.ServerName:
                            break;
                        case MqttServerPropertyType.ServerUrl:
                            break;
                        case MqttServerPropertyType.Port:
                            break;
                        case MqttServerPropertyType.IsConnect:
                            mqttServerItem.IsConnect=e.MqttServer.IsConnect;
                            break;
                        case MqttServerPropertyType.Username:
                            break;
                        case MqttServerPropertyType.Password:
                            break;
                        case MqttServerPropertyType.IsActive:
                            break;
                        case MqttServerPropertyType.SubscribeTopic:
                            break;
                        case MqttServerPropertyType.PublishTopic:
                            break;
                        case MqttServerPropertyType.ClientId:
                            break;
                        case MqttServerPropertyType.MessageFormat:
                            break;
                        case MqttServerPropertyType.MessageHeader:
                            break;
                        case MqttServerPropertyType.MessageContent:
                            break;
                        case MqttServerPropertyType.MessageFooter:
                            break;
                        case MqttServerPropertyType.All:
                            break;
                        default:
                            break;
                    }
                }

            }
            else
            {
                _logger?.LogWarning("在Mqtt服务器队列中找不到ID为 {MqttServer} 的变量，无法更新值", e.MqttServer.Id);
            }
        }));

    }

    private  void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
    {
        _logger?.LogDebug("接收到数据加载完成事件，成功: {IsSuccess}", e.IsSuccess);
        
        if (e.IsSuccess)
        {
            _logger?.LogInformation("开始加载所有数据项");
            
            _wpfDataService.DeviceDataService.LoadAllDevices();
            _logger?.LogDebug("设备数据加载完成");
            
            _wpfDataService.VariableTableDataService.LoadAllVariableTables();
            _logger?.LogDebug("变量表数据加载完成");
            
            _wpfDataService.VariableDataService.LoadAllVariables();
            _logger?.LogDebug("变量数据加载完成");
            
            _wpfDataService.MenuDataService.LoadAllMenus();
            _logger?.LogDebug("菜单数据加载完成");
            
            _wpfDataService.MqttDataService.LoadMqttServers();
            _logger?.LogDebug("MQTT服务器数据加载完成");
            
            _wpfDataService.MqttAliasDataService.LoadMqttAliases();
            _logger?.LogDebug("MQTT别名加载完成");
            _wpfDataService.TriggerDataService.LoadAllTriggers();
            _logger?.LogDebug("触发器加载完成");
            
            _wpfDataService.LogDataService.LoadAllLog();
            _logger?.LogDebug("日志数据加载完成");
            
            _logger?.LogInformation("所有数据项加载完成");
        }
        else
        {
            _logger?.LogWarning("数据加载失败");
        }
    }

    /// <summary>
    /// 处理变量值变更事件。
    /// </summary>
    private void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e)
    {
        _logger?.LogDebug("接收到变量值变更事件，变量ID: {VariableId}, 新值: {NewValue}, 更新时间: {UpdateTime}", 
            e.Variable.Id, e.Variable.DataValue, e.Variable.UpdatedAt);
        
        // 在UI线程上更新变量值
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            // 查找并更新对应的变量
            if (_dataStorageService.Variables.TryGetValue(e.Variable.Id,out var variableToUpdate))
            {
                variableToUpdate.DataValue = e.Variable.DataValue;
                variableToUpdate.DisplayValue = e.Variable.DisplayValue;
                variableToUpdate.UpdatedAt = e.Variable.UpdatedAt;
            }
            else
            {
                _logger?.LogWarning("在变量存储中找不到ID为 {VariableId} 的变量，无法更新值", e.Variable.Id);
            }
        }));
    }

    

    /// <summary>
    /// 处理LoadMessage消息。
    /// </summary>
    public async Task Receive(LoadMessage message)
    {
        _logger?.LogDebug("接收到LoadMessage消息，消息类型: {MessageType}", message.GetType().Name);
        
        // 这里可以添加加载消息处理的逻辑
        // 目前是空实现，但已记录接收到消息的信息
        
        _logger?.LogDebug("LoadMessage消息处理完成");
    }
}