using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// MQTT数据服务类，负责管理MQTT服务器相关的数据和操作。
/// </summary>
public class MqttDataService : IMqttDataService
{
    private readonly IMapper _mapper;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IMqttManagementService _mqttManagementService;
    private readonly IMenuDataService _menuDataService;
    private readonly IMenuManagementService _menuManagementServiceImpl;
    private readonly IDataStorageService _dataStorageService;


    /// <summary>
    /// MqttDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="mqttAppService">MQTT应用服务实例。</param>
    public MqttDataService(IMapper mapper, IAppDataStorageService appDataStorageService, IMqttManagementService mqttManagementService, IMenuDataService menuDataService, IMenuManagementService menuManagementServiceImpl, IDataStorageService dataStorageService)
    {
        _mapper = mapper;
        _appDataStorageService = appDataStorageService;
        _mqttManagementService = mqttManagementService;
        _menuDataService = menuDataService;
        _menuManagementServiceImpl = menuManagementServiceImpl;
        _dataStorageService = dataStorageService;
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
                _dataStorageService.MqttServers.TryAdd(mqttServerDto.Id, _mapper.Map<MqttServerItemViewModel>(mqttServerDto));
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

        var addMqttServerDto = await _mqttManagementService.CreateMqttServerAsync(_mapper.Map<MqttServerDto>(mqttServer));

        MqttServerItemViewModel mqttServerItem = _mapper.Map<MqttServerItemViewModel>(addMqttServerDto);

        _dataStorageService.MqttServers.Add(mqttServerItem.Id, mqttServerItem);


        var mqttRootMenu = _dataStorageService.Menus.FirstOrDefault(m => m.Header == "Mqtt服务器");

        if (mqttRootMenu is not null)
        {
            var mqttServerMenu = new MenuBeanDto()
            {
                Header = mqttServerItem.ServerName,
                TargetId = mqttServerItem.Id,
                ParentId = mqttRootMenu.Id,
                Icon = "\uE753", // 使用设备图标
                MenuType = MenuType.MqttServerMenu,
                TargetViewKey = nameof(MqttServerDetailViewModel),
            };
            await _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(mqttServerMenu));
        }

        return mqttServerItem;
    }

    /// <summary>
    /// 更新MQTT服务器。
    /// </summary>
    public async Task<bool> UpdateMqttServer(MqttServerItemViewModel mqttServer)
    {
        var dto = _mapper.Map<MqttServerDto>(mqttServer);
        var result = await _mqttManagementService.UpdateMqttServerAsync(dto);

        if (result > 0)
        {
            // 更新菜单项
            var menu = _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.MqttServerMenu && m.TargetId == mqttServer.Id);
            if (menu != null)
            {
                // 更新菜单标题
                menu.Header = mqttServer.ServerName;

                // 使用菜单管理服务更新菜单
                var menuDto = _mapper.Map<MenuBeanDto>(menu);
                await _menuManagementServiceImpl.UpdateMenuAsync(menuDto);
            }
        }

        return result > 0;
    }

    /// <summary>
    /// 删除MQTT服务器。
    /// </summary>
    public async Task<bool> DeleteMqttServer(MqttServerItemViewModel mqttServer)
    {
        // 从数据库和内存中删除MQTT服务器
        var result = await _mqttManagementService.DeleteMqttServerAsync(mqttServer.Id);

        if (result)
        {
            // 从界面删除MQTT服务器菜单
            var mqttServerMenu = _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.MqttServerMenu && m.TargetId == mqttServer.Id);

            if (mqttServerMenu != null)
            {
              await  _menuDataService.DeleteMenuItem(mqttServerMenu);
            }

            // 从界面删除MQTT服务器
            _dataStorageService.MqttServers.Remove(mqttServer.Id);
        }

        return result;
    }
}