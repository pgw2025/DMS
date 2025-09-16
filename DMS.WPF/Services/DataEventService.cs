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
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 数据事件服务类，负责处理数据变更事件和消息传递。
/// </summary>
public class DataEventService : IDataEventService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IWPFDataService _wpfDataService;

    /// <summary>
    /// DataEventService类的构造函数。
    /// </summary>
    public DataEventService(IMapper mapper,
        IDataStorageService dataStorageService, 
        IEventService eventService,
        IAppDataCenterService appDataCenterService,
        IWPFDataService wpfDataService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _eventService = eventService;
        _appDataCenterService = appDataCenterService;
        _wpfDataService = wpfDataService;
        
        // 监听变量值变更事件
        _eventService.OnVariableValueChanged += OnVariableValueChanged;
        _appDataCenterService.DataLoaderService.OnLoadDataCompleted += OnLoadDataCompleted;
        // 监听日志变更事件
        // _appDataCenterService.OnLogChanged += _logDataService.OnNlogChanged;
    }
    
    private  void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            _wpfDataService.DeviceDataService.LoadAllDevices();
            _wpfDataService.VariableTableDataService.LoadAllVariableTables();
            _wpfDataService.VariableDataService.LoadAllVariables();
            _wpfDataService.MenuDataService.LoadAllMenus();
            _wpfDataService.MqttDataService.LoadMqttServers();
            _wpfDataService.LogDataService.LoadAllLog();
        }
        
    }

    /// <summary>
    /// 处理变量值变更事件。
    /// </summary>
    private void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e)
    {
        // 在UI线程上更新变量值
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            // 查找并更新对应的变量
            
            if (_dataStorageService.Variables.TryGetValue(e.VariableId,out var variableToUpdate))
            {
                variableToUpdate.DataValue = e.NewValue;
                variableToUpdate.DisplayValue = e.NewValue;
                variableToUpdate.UpdatedAt = e.UpdateTime;
            }
        }));
    }

    

    /// <summary>
    /// 处理LoadMessage消息。
    /// </summary>
    public async Task Receive(LoadMessage message)
    {
       
    }
}