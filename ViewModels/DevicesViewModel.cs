using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Excptions;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly DeviceRepository _deviceRepository;
    private readonly ILogger<DevicesViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly DataServices _dataServices;

    [ObservableProperty] private ObservableCollection<Device> _devices;
    private readonly MenuRepository _menuRepository;

    public DevicesViewModel(
        ILogger<DevicesViewModel> logger, IDialogService dialogService, DataServices dataServices
    )
    {
        _deviceRepository = new DeviceRepository();
        _menuRepository = new MenuRepository();
        _logger = logger;
        _dialogService = dialogService;
        _dataServices = dataServices;

        WeakReferenceMessenger.Default.Send<LoadMessage>(new LoadMessage(LoadTypes.Devices));
        _dataServices.OnDeviceListChanged += (devices) => { Devices = new ObservableCollection<Device>(devices); };
    }

    public async Task OnLoadedAsync()
    {
    }

    [RelayCommand]
    public async void AddDevice()
    {
        Device device = null;
        try
        {
            device = await _dialogService.ShowAddDeviceDialog();
            if (device != null)
            {
                if (await _deviceRepository.Add(device))
                {
                    var msg = $"添加设备成功：{device.Name}";
                    _logger.LogInformation(msg);
                    
                    bool addMenuRes = await _menuRepository.AddDeviceMenu(device);
                    if (addMenuRes)
                    {
                        // 通知更新菜单
                        WeakReferenceMessenger.Default.Send<UpdateMenuMessage>(new UpdateMenuMessage(0));
                        NotificationHelper.ShowMessage(msg, NotificationType.Success);
                    }
                    else
                    {
                        var msgerr = $"给设备添加菜单失败：{device.Name}";
                        _logger.LogInformation(msgerr);
                        NotificationHelper.ShowMessage(msgerr, NotificationType.Error);
                    }
                }
                else
                {
                    var msg = $"添加设备失败：{device.Name}";
                    _logger.LogInformation(msg);
                    NotificationHelper.ShowMessage(msg, NotificationType.Error);
                }
            }
        }
        catch (Exception e)
        {
            var msg = $"添加设备失败：{e.Message}";
            _logger.LogError(msg);
            NotificationHelper.ShowMessage(msg, NotificationType.Success);
        }
    }

    [RelayCommand]
    public void NavigateVt()
    {
    }


    public override async void OnLoaded()
    {
        await OnLoadedAsync();
    }
}