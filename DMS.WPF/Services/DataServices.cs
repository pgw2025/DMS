using System.Collections.ObjectModel;
using System.Windows;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.Message;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 数据服务类，负责从数据库加载和管理各种数据，并提供数据变更通知。
/// 继承自ObservableRecipient，可以接收消息；实现IRecipient<LoadMessage>，处理加载消息。
/// </summary>
public partial class DataServices : ObservableObject, IRecipient<LoadMessage>, IDisposable
{
    private readonly IMapper _mapper;
    private readonly IDataCenterService _dataCenterService;
    private readonly IMqttAppService _mqttAppService;


    // 设备列表，使用ObservableProperty特性，当值改变时会自动触发属性变更通知。
    [ObservableProperty]
    private ObservableCollection<DeviceItemViewModel> _devices;

    // 变量表列表。
    [ObservableProperty]
    private ObservableCollection<VariableTableItemViewModel> _variableTables;

    // 变量数据列表。
    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _variables;

    // 菜单树列表。
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menus;

    // 菜单树列表。
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menuTrees;

    // MQTT配置列表。
    [ObservableProperty]
    private ObservableCollection<MqttServerItemViewModel> _mqttServers;


    // 设备列表变更事件，当设备列表数据更新时触发。
    public event Action<List<Device>> OnDeviceListChanged;

    // 菜单树列表变更事件，当菜单树数据更新时触发。
    public event Action<List<MenuBean>> OnMenuTreeListChanged;

    // MQTT列表变更事件，当MQTT配置数据更新时触发。
    public event Action<List<MqttServerDto>> OnMqttListChanged;

    /// <summary>
    /// 处理变量值变更事件
    /// </summary>
    private void OnVariableValueChanged(object sender, VariableValueChangedEventArgs e)
    {
        // 在UI线程上更新变量值
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            // 查找并更新对应的变量
            var variableToUpdate = Variables.FirstOrDefault(v => v.Id == e.VariableId);
            if (variableToUpdate != null)
            {
                variableToUpdate.DataValue = e.NewValue;
                variableToUpdate.DisplayValue = e.NewValue;
                variableToUpdate.UpdatedAt = e.UpdateTime;
            }
        }));
    }


    /// <summary>
    /// DataServices类的构造函数。
    /// 初始化各个数据仓库。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="dataCenterService">数据服务中心实例。</param>
    /// <param name="mqttAppService">MQTT应用服务实例。</param>
    public DataServices(IMapper mapper, IDataCenterService dataCenterService, IMqttAppService mqttAppService)
    {
        _mapper = mapper;
        _dataCenterService = dataCenterService;
        _mqttAppService = mqttAppService;
        Devices = new ObservableCollection<DeviceItemViewModel>();
        VariableTables = new ObservableCollection<VariableTableItemViewModel>();
        Variables = new ObservableCollection<VariableItemViewModel>();
        Menus = new ObservableCollection<MenuItemViewModel>();
        MenuTrees = new ObservableCollection<MenuItemViewModel>();
        MqttServers = new ObservableCollection<MqttServerItemViewModel>();
        
        // 监听变量值变更事件
        _dataCenterService.VariableValueChanged += OnVariableValueChanged;
        
        // 注册消息接收
        // WeakReferenceMessenger.Register<LoadMessage>(this, (r, m) => r.Receive(m));
    }


    /// <summary>
    /// 异步加载设备数据，并以高效的方式更新UI集合。
    /// 此方法会比较新旧数据，只对有变化的设备进行更新、添加或删除，避免不必要的UI刷新。
    /// </summary>
    public async Task LoadAllDatas()
    {
        Devices = _mapper.Map<ObservableCollection<DeviceItemViewModel>>(_dataCenterService.Devices.Values);
        foreach (var device in Devices)
        {
            foreach (var variableTable in device.VariableTables)
            {
                VariableTables.Add(variableTable);
                foreach (var variable in variableTable.Variables)
                {
                    Variables.Add(variable);
                }
            }
        }

        Menus = _mapper.Map<ObservableCollection<MenuItemViewModel>>(_dataCenterService.Menus.Values);

        BuildMenuTrees();
    }

    public async Task<CreateDeviceWithDetailsDto> AddDevice(CreateDeviceWithDetailsDto dto)
    {
        var addDto = await _dataCenterService.CreateDeviceWithDetailsAsync(dto);
        //更新当前界面
        Devices.Add(_mapper.Map<DeviceItemViewModel>(addDto.Device));
        AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.DeviceMenu));
        await AddVariableTable(addDto.VariableTable);
        AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.VariableTableMenu));
        //更新数据中心
        _dataCenterService.AddDeviceToMemory(addDto.Device);
        _dataCenterService.AddVariableTableToMemory(addDto.VariableTable);
        _dataCenterService.AddMenuToMemory(addDto.DeviceMenu);
        _dataCenterService.AddMenuToMemory(addDto.VariableTableMenu);

        BuildMenuTrees();


        return addDto;
    }


    public async Task<bool> DeleteDevice(DeviceItemViewModel device)
    {
        if (!await _dataCenterService.DeleteDeviceByIdAsync(device.Id))
        {
            return false;
        }

        _dataCenterService.RemoveDeviceFromMemory(device.Id);


        // 1. 删除与设备关联的所有变量表及其变量
        foreach (var variableTable in device.VariableTables)
        {
            // 删除与当前变量表关联的所有变量
            DeleteVariableTable(variableTable);
        }

        // 2. 删除设备
        Devices.Remove(device);

        // 3. 删除与设备关联的菜单项
        var deviceMenu = Menus.FirstOrDefault(m => m.MenuType == MenuType.DeviceMenu && m.TargetId == device.Id);
        DeleteMenuItem(deviceMenu);


        return true;
    }


    public async Task<bool> UpdateDevice(DeviceItemViewModel device)
    {
        if (!_dataCenterService.Devices.TryGetValue(device.Id, out var deviceDto))
        {
            return false;
        }

        _mapper.Map(device, deviceDto);
        if (await _dataCenterService.UpdateDeviceAsync(deviceDto) > 0)
        {
            var menu = Menus.FirstOrDefault(m =>
                                                m.MenuType == MenuType.DeviceMenu &&
                                                m.TargetId == device.Id);
            if (menu != null)
            {
                menu.Header = device.Name;
            }
        }

        return true;
    }

    
    public async Task<bool> AddVariableTable(VariableTableDto variableTableDto,
                                             MenuBeanDto menuDto = null, bool isAddDb = false)
    {
        if (variableTableDto == null)
            return false;

        if (isAddDb && menuDto != null)
        {
            CreateVariableTableWithMenuDto createDto = new CreateVariableTableWithMenuDto();
            createDto.VariableTable = variableTableDto;
            createDto.DeviceId = variableTableDto.DeviceId;
            createDto.Menu = menuDto;
            var resDto = await _dataCenterService.CreateVariableTableAsync(createDto);
            _mapper.Map(resDto.VariableTable, variableTableDto);
            AddMenuItem(_mapper.Map<MenuItemViewModel>(resDto.Menu));
        }
        
        _dataCenterService.AddVariableTableToMemory(variableTableDto);

        var device = Devices.FirstOrDefault(d => d.Id == variableTableDto.DeviceId);
        if (device != null)
        {
            var variableTableItemViewModel = _mapper.Map<VariableTableItemViewModel>(variableTableDto);
            variableTableItemViewModel.Device = device;
            device.VariableTables.Add(variableTableItemViewModel);
            VariableTables.Add(variableTableItemViewModel);
        }

        return true;
    }

    public async Task<bool> UpdateVariableTable(VariableTableItemViewModel variableTable)
    {
        if (variableTable==null)
        {
            return false;
        }

        var variableTableDto = _mapper.Map<VariableTableDto>(variableTable);
        if (await _dataCenterService.UpdateVariableTableAsync(variableTableDto) > 0)
        {
            
            _dataCenterService.UpdateVariableTableInMemory(variableTableDto);
            
            var menu = Menus.FirstOrDefault(m =>
                                                             m.MenuType == MenuType.VariableTableMenu &&
                                                             m.TargetId == variableTable.Id);
            if (menu != null)
            {
                menu.Header = variableTable.Name;
            }
            
            return true;
        }

        return false;

    }
    public async Task<bool> DeleteVariableTable(VariableTableItemViewModel variableTable, bool isDeleteDb = false)
    {
        if (variableTable == null)
        {
            return false;
        }

        if (isDeleteDb)
        {
            if (!await _dataCenterService.DeleteVariableTableAsync(variableTable.Id))
            {
                return false;
            }
        }

        // 删除与当前变量表关联的所有变量
        foreach (var variable in variableTable.Variables)
        {
            Variables.Remove(variable);
        }
        
        _dataCenterService.RemoveVariableTableFromMemory(variableTable.Id);

        var variableTableMenu
            = Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu && m.TargetId == variableTable.Id);
        DeleteMenuItem(variableTableMenu);
        // 删除变量表
        VariableTables.Remove(variableTable);
        variableTable.Device.VariableTables.Remove(variableTable);
        return true;
    }


    private void BuildMenuTrees()
    {
        MenuTrees.Clear();
        // 遍历所有菜单项，构建树形结构
        foreach (var menu in Menus)
        {
            var parentMenu = Menus.FirstOrDefault(m => m.Id == menu.ParentId);
            // 检查是否有父ID，并且父ID不为0（通常0或null表示根节点）
            if (parentMenu != null && menu.ParentId != 0)
            {
                // 将当前菜单添加到父菜单的Children列表中
                if (!parentMenu.Children.Contains(menu))
                {
                    parentMenu.Children.Add(menu);
                }
            }
            else
            {
                // 如果没有父ID，则这是一个根菜单
                MenuTrees.Add(menu);
            }
        }
    }


    public void AddMenuItem(MenuItemViewModel menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        var deviceMenu = Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId);
        if (deviceMenu != null)
        {
            deviceMenu.Children.Add(menuItemViewModel);
            Menus.Add(menuItemViewModel);
        }
    }

    public void AddVariable(VariableItemViewModel variableItem)
    {
        if (variableItem == null)
        {
            return;
        }

        var variableTable = VariableTables.FirstOrDefault(d => d.Id == variableItem.VariableTableId);
        if (variableTable != null)
        {
            variableTable.Variables.Add(variableItem);
            Variables.Add(variableItem);
        }
    }

    private void DeleteMenuItem(MenuItemViewModel? menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        // 从扁平菜单列表中移除
        Menus.Remove(menuItemViewModel);

        // 从树形结构中移除
        if (menuItemViewModel.ParentId.HasValue && menuItemViewModel.ParentId.Value != 0)
        {
            // 如果有父菜单，从父菜单的Children中移除
            var parentMenu = Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId.Value);
            parentMenu?.Children.Remove(menuItemViewModel);
        }
        else
        {
            // 如果是根菜单，从MenuTrees中移除
            MenuTrees.Remove(menuItemViewModel);
        }
    }


    public void DeleteVariable(int id)
    {
        var variableItem = Variables.FirstOrDefault(v => v.Id == id);
        if (variableItem == null)
        {
            return;
        }

        var variableTable = VariableTables.FirstOrDefault(vt => vt.Id == variableItem.VariableTableId);

        variableTable.Variables.Remove(variableItem);

        Variables.Remove(variableItem);
    }

    /// <summary>
    /// 异步加载所有MQTT服务器数据
    /// </summary>
    public async Task LoadMqttServers(IMqttAppService mqttAppService)
    {
        try
        {
            var mqttServerDtos = await mqttAppService.GetAllMqttServersAsync();
            MqttServers = _mapper.Map<ObservableCollection<MqttServerItemViewModel>>(mqttServerDtos);
            OnMqttListChanged?.Invoke(mqttServerDtos);
        }
        catch (Exception ex)
        {
            // 记录异常或处理错误
            Console.WriteLine($"加载MQTT服务器数据时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加MQTT服务器
    /// </summary>
    public async Task<MqttServerItemViewModel> AddMqttServer(IMqttAppService mqttAppService, MqttServerItemViewModel mqttServer)
    {
        var dto = _mapper.Map<MqttServerDto>(mqttServer);
        var id = await mqttAppService.CreateMqttServerAsync(dto);
        dto.Id = id;
        
        var mqttServerItem = _mapper.Map<MqttServerItemViewModel>(dto);
        MqttServers.Add(mqttServerItem);
        
        return mqttServerItem;
    }

    /// <summary>
    /// 更新MQTT服务器
    /// </summary>
    public async Task<bool> UpdateMqttServer(IMqttAppService mqttAppService, MqttServerItemViewModel mqttServer)
    {
        var dto = _mapper.Map<MqttServerDto>(mqttServer);
        await mqttAppService.UpdateMqttServerAsync(dto);
        return true;
    }

    /// <summary>
    /// 删除MQTT服务器
    /// </summary>
    public async Task<bool> DeleteMqttServer(IMqttAppService mqttAppService, MqttServerItemViewModel mqttServer)
    {
        await mqttAppService.DeleteMqttServerAsync(mqttServer.Id);
        MqttServers.Remove(mqttServer);
        return true;
    }

    /// <summary>
    /// 处理LoadMessage消息
    /// </summary>
    /// <param name="message">加载消息</param>
    public void Receive(LoadMessage message)
    {
        switch (message.LoadType)
        {
            case LoadTypes.Devices:
                // 设备数据已在IDataCenterService中处理
                break;
            case LoadTypes.Menu:
                // 菜单数据已在IDataCenterService中处理
                break;
            case LoadTypes.Mqtts:
                _ = Task.Run(async () => await LoadMqttServers(_mqttAppService));
                break;
            case LoadTypes.All:
                // 加载所有数据
                LoadAllDatas();
                _ = Task.Run(async () => await LoadMqttServers(_mqttAppService));
                break;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 取消事件订阅
        if (_dataCenterService != null)
        {
            _dataCenterService.VariableValueChanged -= OnVariableValueChanged;
        }
    }
}