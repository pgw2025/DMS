using System.Windows.Threading;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Events;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using Opc.Ua;

namespace DMS.WPF.Services;

/// <summary>
/// 触发器数据服务类，负责管理触发器相关的数据和操作。
/// </summary>
public class TriggerDataService : ITriggerDataService
{
    private readonly IMapper _mapper;
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly Dispatcher _uiDispatcher;

    /// <summary>
    /// TriggerDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataCenterService">数据服务中心实例。</param>
    /// <param name="appDataStorageService">应用数据存储服务实例。</param>
    /// <param name="dataStorageService">数据存储服务实例。</param>
    /// <param name="eventService">事件服务实例。</param>
    /// <param name="notificationService">通知服务实例。</param>
    public TriggerDataService(IMapper mapper, IAppDataCenterService appDataCenterService,
                              IAppDataStorageService appDataStorageService, IDataStorageService dataStorageService,
                              IEventService eventService, INotificationService notificationService)
    {
        _mapper = mapper;
        _appDataCenterService = appDataCenterService;
        _appDataStorageService = appDataStorageService;
        _dataStorageService = dataStorageService;
        _eventService = eventService;
        _notificationService = notificationService;
        _uiDispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    /// 加载所有触发器数据。
    /// </summary>
    public void LoadAllTriggers()
    {
        foreach (var triggerDto in _appDataStorageService.Triggers.Values)
        {
            _dataStorageService.Triggers.Add(triggerDto.Id, _mapper.Map<TriggerItem>(triggerDto));
        }
    }

    /// <summary>
    /// 添加触发器。
    /// </summary>
    public async Task<TriggerItem> AddTrigger(TriggerItem dto)
    {
        // 添加null检查
        if (dto == null)
            return null;

        var addDto
            = await _appDataCenterService.TriggerManagementService.CreateTriggerAsync(
                _mapper.Map<TriggerDefinitionDto>(dto));

        // 添加null检查
        if (addDto == null)
        {
            return null;
        }

        // 给界面添加触发器
        var addItem = _mapper.Map<TriggerItem>(addDto);
        _dataStorageService.Triggers.Add(addDto.Id, addItem);

        return addItem;
    }

    /// <summary>
    /// 删除触发器。
    /// </summary>
    public async Task<bool> DeleteTrigger(TriggerItem trigger)
    {
        // 从数据库删除触发器数据
        if (!await _appDataCenterService.TriggerManagementService.DeleteTriggerAsync(trigger.Id))
        {
            return false;
        }

        // 从界面删除触发器
        _dataStorageService.Triggers.Remove(trigger.Id);

        return true;
    }

    /// <summary>
    /// 更新触发器。
    /// </summary>
    public async Task<bool> UpdateTrigger(TriggerItem trigger)
    {
        if (!_appDataStorageService.Triggers.TryGetValue(trigger.Id, out var triggerDto))
        {
            return false;
        }

        _mapper.Map(trigger, triggerDto);
        if (await _appDataCenterService.TriggerManagementService.UpdateTriggerAsync(trigger.Id, triggerDto) != null)
        {
            return true;
        }

        return false;
    }
}