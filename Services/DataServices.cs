using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.ViewModels;
using SqlSugar;

namespace PMSWPF.Services;

public partial class DataServices : ObservableRecipient, IRecipient<LoadMessage>
{
    private readonly ILogger<DataServices> _logger;
    [ObservableProperty] private List<Device> _devices;
    [ObservableProperty] private List<VariableTable> _variableTables;
    [ObservableProperty] private List<MenuBean> menuTrees;
    [ObservableProperty] private List<Mqtt> _mqtts;
    private readonly DeviceRepository _deviceRepository;
    private readonly MenuRepository _menuRepository;
    private readonly MqttRepository _mqttRepository;


    public event Action<List<Device>> OnDeviceListChanged;
    public event Action<List<MenuBean>> OnMenuTreeListChanged;
    public event Action<List<Mqtt>> OnMqttListChanged;


    partial void OnDevicesChanged(List<Device> devices)
    {
        OnDeviceListChanged?.Invoke(devices);
    }

    partial void OnMenuTreesChanged(List<MenuBean> MenuTrees)
    {
        OnMenuTreeListChanged?.Invoke(MenuTrees);
    }

    partial void OnMqttsChanged(List<Mqtt> mqtts)
    {
        OnMqttListChanged?.Invoke(mqtts);
    }


    public DataServices(ILogger<DataServices> logger)
    {
        _logger = logger;
        IsActive = true;
        _deviceRepository = new DeviceRepository();
        _menuRepository = new MenuRepository();
        _mqttRepository = new MqttRepository();
    }


    /// <summary>
    /// 接受加载消息，收到消息后从数据库加载对应的数据
    /// </summary>
    /// <param name="message">消息的类型，如加载菜单LoadMessage.Menu</param>
    /// <exception cref="ArgumentException"></exception>
    public async void Receive(LoadMessage message)
    {
        try
        {
            switch (message.LoadType)
            {
                case LoadTypes.All:
                    await LoadDevices();
                    await LoadMenus();
                    await LoadMqtts();
                    break;
                case LoadTypes.Devices:
                    await LoadDevices();
                    break;
                case LoadTypes.Menu:
                    await LoadMenus();
                    break;
                case LoadTypes.Mqtts:
                    await LoadMqtts();
                    break;
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"加载数据出现了错误：{e.Message}");
            _logger.LogError($"加载数据出现了错误：{e}");
        }
    }

    private async Task LoadDevices()
    {
        Devices = await _deviceRepository.GetAll();
    }

    private async Task LoadMenus()
    {
        MenuTrees = await _menuRepository.GetMenuTrees();
        foreach (MenuBean menu in MenuTrees)
        {
            MenuHelper.MenuAddParent(menu);
            DataServicesHelper.SortMenus(menu);
        }
    }

    private async Task LoadMqtts()
    {
        Mqtts = await _mqttRepository.GetAll();
    }
}