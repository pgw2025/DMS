using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Application.Services.Triggers; // 添加触发器服务引用
using DMS.Core.Enums;
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
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IWPFDataService _wpfDataService;
    private readonly ITriggerEvaluationService _triggerEvaluationService; // 新增依赖

    /// <summary>
    /// DataEventService类的构造函数。
    /// </summary>
    public DataEventService(IMapper mapper,
        IDataStorageService dataStorageService, 
        IAppDataCenterService appDataCenterService,
        IWPFDataService wpfDataService,
        ITriggerEvaluationService triggerEvaluationService // 新增参数
       )
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataCenterService = appDataCenterService;
        _wpfDataService = wpfDataService;
        _triggerEvaluationService = triggerEvaluationService; // 赋值
        
        // 监听变量值变更事件
        _appDataCenterService.VariableManagementService.OnVariableValueChanged += OnVariableValueChanged;
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
    private async void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e) // 改为 async void 以便调用 await
    {
        // 在UI线程上更新变量值
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            // 查找并更新对应的变量
            var variableToUpdate = _dataStorageService.Variables.FirstOrDefault(v => v.Id == e.VariableId);
            if (variableToUpdate != null)
            {
                variableToUpdate.DataValue = e.NewValue;
                variableToUpdate.DisplayValue = e.NewValue;
                variableToUpdate.UpdatedAt = e.UpdateTime;
            }
        }));

        // 在后台线程上调用触发器评估服务
        // 使用 Task.Run 将其放到线程池线程上执行，避免阻塞 UI 线程
        // 注意：这里调用的是 Fire-and-forget，因为我们不等待结果。
        // 如果将来需要处理执行结果或错误，可以考虑使用 async Task 并在适当的地方等待。
        _ = Task.Run(async () =>
        {
            try
            {
                await _triggerEvaluationService.EvaluateTriggersAsync(e.VariableId, e.NewValue);
            }
            catch (Exception ex)
            {
                // Log the exception appropriately.
                // Since this is fire-and-forget, we must handle exceptions internally.
                // You might have a logging service injected or use a static logger.
                // For now, let's assume a static logger or inline comment.
                System.Diagnostics.Debug.WriteLine($"Error evaluating triggers for variable {e.VariableId}: {ex}");
                // Consider integrating with your logging framework (e.g., NLog) here.
            }
        });
    }

    

    /// <summary>
    /// 处理LoadMessage消息。
    /// </summary>
    public async Task Receive(LoadMessage message)
    {
       
    }
}