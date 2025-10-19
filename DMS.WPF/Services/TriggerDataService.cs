using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Core.Models.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using Opc.Ua;
using System.Windows.Threading;

namespace DMS.WPF.Services;

/// <summary>
/// 触发器数据服务类，负责管理触发器相关的数据和操作。
/// </summary>
public class TriggerDataService : ITriggerDataService
{
    private readonly IMapper _mapper;
    private readonly IAppCenterService _appCenterService;
    private readonly IAppStorageService _appStorageService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly Dispatcher _uiDispatcher;

    /// <summary>
    /// TriggerDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appCenterService">数据服务中心实例。</param>
    /// <param name="appStorageService">应用数据存储服务实例。</param>
    /// <param name="dataStorageService">数据存储服务实例。</param>
    /// <param name="eventService">事件服务实例。</param>
    /// <param name="notificationService">通知服务实例。</param>
    public TriggerDataService(IMapper mapper, IAppCenterService appCenterService,
                              IAppStorageService appStorageService, IDataStorageService dataStorageService,
                              IEventService eventService, INotificationService notificationService)
    {
        _mapper = mapper;
        _appCenterService = appCenterService;
        _appStorageService = appStorageService;
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
        _dataStorageService.Triggers.Clear();
        foreach (var triggerDto in _appStorageService.Triggers.Values)
        {
            _dataStorageService.Triggers.Add(triggerDto.Id, _mapper.Map<TriggerItem>(triggerDto));
        }
    }

    /// <summary>
    /// 添加触发器。
    /// </summary>
    public async Task<TriggerItem> AddTrigger(TriggerItem triggerItem)
    {
        // 添加null检查
        if (triggerItem == null)
            return null;

        var addDto
            = await _appCenterService.TriggerManagementService.CreateTriggerAsync(
                _mapper.Map<Trigger>(triggerItem));

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
        if (!await _appCenterService.TriggerManagementService.DeleteTriggerAsync(trigger.Id))
        {
            return false;
        }

        // 从界面删除触发器
        _dataStorageService.Triggers.Remove(trigger.Id);

        return true;
    }

    /// <summary>
    /// 添加触发器及其关联菜单。
    /// </summary>
    public async Task<CreateTriggerWithMenuDto> AddTriggerWithMenu(CreateTriggerWithMenuDto dto)
    {
        // 添加null检查
        if (dto == null || dto.Trigger == null)
            return null;

        try
        {
            // 首先添加触发器
            var createdTrigger = await _appCenterService.TriggerManagementService.CreateTriggerAsync(dto.Trigger);
            if (createdTrigger == null)
                return null;

            
            var  parentMenu=_appStorageService.Menus.Values.OrderBy(m=>m.Id).FirstOrDefault(m=>m.MenuType==MenuType.TriggerMenu);
            if (parentMenu is not null)
            {
                // 将菜单关联到触发器
                dto.TriggerMenu.TargetId = createdTrigger.Id;
                dto.TriggerMenu.MenuType = MenuType.TriggerMenu;

            }


           
            
            // 添加到UI数据存储
            var addItem = _mapper.Map<TriggerItem>(createdTrigger);
            _dataStorageService.Triggers.Add(createdTrigger.Id, addItem);

            return dto;
        }
        catch (Exception ex)
        {
            _notificationService?.ShowError($"添加触发器及菜单时发生错误：{ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// 更新触发器。
    /// </summary>
    public async Task<bool> UpdateTrigger(TriggerItem trigger)
    {
        if (!_appStorageService.Triggers.TryGetValue(trigger.Id, out var triggerDto))
        {
            return false;
        }

        _mapper.Map(trigger, triggerDto);
        if (await _appCenterService.TriggerManagementService.UpdateTriggerAsync(trigger.Id, triggerDto) != null)
        {
            return true;
        }

        return false;
    }
}