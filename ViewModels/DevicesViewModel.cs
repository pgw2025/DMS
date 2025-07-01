using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
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
    [ObservableProperty] private Device _selectedDevice;
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

        MessageHelper.SendLoadMessage(LoadTypes.Devices);
        _dataServices.OnDeviceListChanged += (devices) => { Devices = new ObservableCollection<Device>(devices); };
    }

    /// <summary>
    /// 添加设备
    /// </summary>
    [RelayCommand]
    public async void AddDevice()
    {
        Device device = null;
        try
        {
            device = await _dialogService.ShowAddDeviceDialog();
            if (device != null)
            {
                device = await _deviceRepository.Add(device);
                if (device != null)
                {
                    var msg = $"添加设备成功：{device.Name}";
                    _logger.LogInformation(msg);

                    bool addMenuRes = await _menuRepository.AddDeviceMenu(device);
                    if (addMenuRes)
                    {
                        // 通知更新菜单
                        MessageHelper.SendLoadMessage(LoadTypes.Menu);
                        MessageHelper.SendLoadMessage(LoadTypes.Devices);
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
            NotificationHelper.ShowMessage(msg, NotificationType.Error);
        }
    }


    /// <summary>
    /// 编辑设备
    /// </summary>
    [RelayCommand]
    public async void EditDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowMessage("你没有选择任何设备，请选择设备后再点击编辑设备", NotificationType.Error);
                return;
            }

            var editDievce = await _dialogService.ShowEditDeviceDialog(SelectedDevice);
            if (editDievce != null)
            {
                var res = await _deviceRepository.Edit(editDievce);
                await _dataServices.UpdateMenuForDevice(editDievce);

                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"编辑设备的过程中发生错误：{e.Message}", NotificationType.Error);
            _logger.LogError($"编辑设备的过程中发生错误：{e}");
        }
    }

    [RelayCommand]
    public async void DeleteDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowMessage("你没有选择任何设备，请选择设备后再点击删除设备", NotificationType.Error);
                return;
            }

            string msg = $"确认要删除设备名为:{SelectedDevice.Name}";
            var isDel = await _dialogService.ShowConfrimeDialog("删除设备", msg, "删除设备");
            if (isDel)
            {
                var defDeviceRes = await _deviceRepository.DeleteById(SelectedDevice.Id);
                var defMenuRes = await _dataServices.DeleteMenuForDevice(SelectedDevice);
                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
                NotificationHelper.ShowMessage($"删除设备成功,设备名：{SelectedDevice.Name}", NotificationType.Success);
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"编辑设备的过程中发生错误：{e.Message}", NotificationType.Error);
            _logger.LogError($"编辑设备的过程中发生错误：{e}");
        }
    }
    
    [RelayCommand]
    public void NavigateVt()
    {
    }


    public override async void OnLoaded()
    {
    }
}