using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Core.Models;
using DMS.Core.Models.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using DMS.WPF.ViewModels;
using HandyControl.Data;
using Opc.Ua;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DMS.WPF.Services;

/// <summary>
/// 触发器数据服务类，负责管理触发器相关的数据和操作。
/// </summary>
public class TriggerDataService : ITriggerDataService
{
    private readonly IMapper _mapper;
    private readonly IAppCenterService _appCenterService;
    private readonly IMenuDataService _menuDataService;
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
                              IMenuDataService menuDataService,
                              IAppStorageService appStorageService, IDataStorageService dataStorageService,
                              IEventService eventService, INotificationService notificationService)
    {
        _mapper = mapper;
        _appCenterService = appCenterService;
        _menuDataService = menuDataService;
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
        if (triggerItem is null) return null;

        var addDto
            = await _appCenterService.TriggerManagementService.AddTriggerAsync(
                _mapper.Map<Core.Models.Triggers.Trigger>(triggerItem));

        // 添加null检查
        if (addDto is null) return null;

        // 给界面添加触发器
        var addItem = _mapper.Map(addDto, triggerItem);

        _dataStorageService.Triggers.Add(triggerItem.Id, triggerItem);

        //添加菜单
        
        var parentMenu=_dataStorageService.Menus.FirstOrDefault(m => m.TargetViewKey == nameof(TriggersViewModel) && m.TargetId == 0);
        if (parentMenu is not null)
        {
            var menuItem = new ItemViewModel.MenuItem()
            {
                Header = triggerItem.Name,
                ParentId=parentMenu.Id,
                MenuType=MenuType.TriggerMenu,
                TargetId=addItem.Id,
                Icon = "\uE945", // 使用触发器图标
                TargetViewKey = nameof(TriggerDetailViewModel),
            };
            await _menuDataService.AddMenuItem(menuItem);

        }
        


        return addItem;
    }

    /// <summary>
    /// 删除触发器。
    /// </summary>
    public async Task<bool> DeleteTrigger(TriggerItem trigger)
    {
        // 从数据库删除触发器数据
        if (await _appCenterService.TriggerManagementService.DeleteTriggerAsync(trigger.Id))
        {
            //删除菜单
            var menu=_dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.TriggerMenu && m.TargetId == trigger.Id);
            if (menu is not null)
            {
              await  _menuDataService.DeleteMenuItem(menu);
            }

            // 从界面删除触发器
            _dataStorageService.Triggers.Remove(trigger.Id);

            return true;
        }
        return false;
    }

    /// <summary>
    /// 添加触发器及其关联菜单。
    /// </summary>
    public async Task<CreateTriggerWithMenuDto> CreateTriggerWithMenu(CreateTriggerWithMenuDto dto)
    {
        // 添加null检查
        if (dto == null || dto.Trigger == null)
            return null;

        try
        {
            // 首先添加触发器
            var createdTrigger = await _appCenterService.TriggerManagementService.CreateTriggerWithMenuAsync(dto);
            if (createdTrigger == null)
                return null;


            // 添加到UI数据存储
            var addItem = _mapper.Map<TriggerItem>(createdTrigger.Trigger);
            _dataStorageService.Triggers.Add(addItem.Id, addItem);



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
    public async Task<bool> UpdateTrigger(TriggerItem triggerItem)
    {
        if (_appStorageService.Triggers.TryGetValue(triggerItem.Id, out var triggerDto))
        {
            _mapper.Map(triggerItem, triggerDto);
            if (await _appCenterService.TriggerManagementService.UpdateTriggerAsync(triggerDto) > 0)
            {
                if (_dataStorageService.Triggers.TryGetValue(triggerItem.Id,out var mTrigger))
                {
                    _mapper.Map(triggerItem,mTrigger);

                    //菜单
                    var menuItem = _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.TriggerMenu && m.TargetId == triggerItem.Id);
                    if (menuItem is not null)
                    {
                        menuItem.Header = triggerItem.Name;
                       await _menuDataService.UpdateMenuItem(menuItem);
                    }

                    return true;
                }

                
            }
        }

        
        

        return false;
    }
}