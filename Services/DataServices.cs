using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;

namespace PMSWPF.Services;

public partial class DataServices:ObservableRecipient,IRecipient<LoadMessage>
{
    private readonly ILogger<DataServices> _logger;
    [ObservableProperty]
    private List<Device> _devices = new List<Device>();
    [ObservableProperty]
    private List<MenuBean> menuBeans = new List<MenuBean>();
    private readonly DeviceRepository _deviceRepository;
    private readonly MenuRepository _menuRepository;

    public event Action<List<Device>> OnDeviceListChanged;
    public event Action<List<MenuBean>> OnMenuListChanged;


     partial void OnDevicesChanged(List<Device> devices)
    {
        OnDeviceListChanged?.Invoke(devices);
    }

    partial void OnMenuBeansChanged(List<MenuBean> menuBeans)
    {
        OnMenuListChanged?.Invoke(menuBeans);
    }
    

    public DataServices(ILogger<DataServices> logger )
    {
        _logger = logger;
        IsActive = true;
         _deviceRepository = new DeviceRepository();
         _menuRepository = new MenuRepository();

    }



    public async void Receive(LoadMessage message)
    {
        
        if (!(message.Value is LoadTypes))
            throw new ArgumentException($"接受到的加载类型错误：{message.Value}");
        try
        {

            switch ((LoadTypes)message.Value )
            {
                case LoadTypes.Devices:
                    Devices= await _deviceRepository.GetAll();
                    break;
                case LoadTypes.Menu:
                    MenuBeans= await _menuRepository.GetMenu();
                    break;
            
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"加载数据出现了错误：{e.Message}");
            _logger.LogError($"加载数据出现了错误：{e.Message}");

        }
        
    }
}