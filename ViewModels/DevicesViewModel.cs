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

    // /// <summary>
    // /// 添加设备
    // /// </summary>
    // [RelayCommand]
    // public async void AddDevice()
    // {
    //     Device device = null;
    //     try
    //     {
    //         device = await _dialogService.ShowAddDeviceDialog();
    //         if (device != null)
    //         {
    //             device = await _deviceRepository.Add(device);
    //             if (device != null)
    //             {
    //                 var msg = $"添加设备成功：{device.Name}";
    //                 _logger.LogInformation(msg);
    //
    //                 bool addMenuRes = await _menuRepository.AddDeviceMenu(device);
    //                 if (addMenuRes)
    //                 {
    //                     // 通知更新菜单
    //                     MessageHelper.SendLoadMessage(LoadTypes.Menu);
    //                     MessageHelper.SendLoadMessage(LoadTypes.Devices);
    //                     NotificationHelper.ShowMessage(msg, NotificationType.Success);
    //                 }
    //                 else
    //                 {
    //                     var msgerr = $"给设备添加菜单失败：{device.Name}";
    //                     _logger.LogInformation(msgerr);
    //                     NotificationHelper.ShowMessage(msgerr, NotificationType.Error);
    //                 }
    //             }
    //             else
    //             {
    //                 var msg = $"添加设备失败：{device.Name}";
    //                 _logger.LogInformation(msg);
    //                 NotificationHelper.ShowMessage(msg, NotificationType.Error);
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         var msg = $"添加设备失败：{e.Message}";
    //         _logger.LogError(msg);
    //         NotificationHelper.ShowMessage(msg, NotificationType.Error);
    //     }
    // }
    
    /// <summary>
/// 辅助方法：处理成功通知和日志
/// </summary>
private void HandleOperationSuccess(string entityType, string operation, string entityName)
{
    var message = $"{entityType}{operation}成功：{entityName}";
    _logger.LogInformation(message);
    NotificationHelper.ShowMessage(message, NotificationType.Success);
}

/// <summary>
/// 辅助方法：处理失败通知和日志
/// </summary>
private void HandleOperationFailure(string entityType, string operation, string entityName, Exception ex = null)
{
    var message = $"{entityType}{operation}失败：{entityName}";
    if (ex != null)
    {
        _logger.LogError(ex, message);
    }
    else
    {
        _logger.LogError(message);
    }
    NotificationHelper.ShowMessage(message, NotificationType.Error);
}

/// <summary>
/// 添加设备
/// </summary>
[RelayCommand]
public async void AddDevice()
{
    try
    {
        // 1. 显示添加设备对话框
        var device = await _dialogService.ShowAddDeviceDialog();
        // 如果用户取消或对话框未返回设备，则直接返回
        if (device == null)
        {
            _logger.LogInformation("用户取消了添加设备操作。");
            return;
        }

        // 2. 将设备添加到数据库
        var addedDevice = await _deviceRepository.Add(device);
        // 如果数据库添加失败
        if (addedDevice == null)
        {
            HandleOperationFailure("设备", "添加", device.Name);
            return; // 提前返回
        }

        // 3. 设备成功添加到数据库，进行菜单添加
        // 这里立即发出成功的通知和日志
        HandleOperationSuccess("设备", "添加", addedDevice.Name);

        // 4. 为新设备添加菜单
        bool menuAddedSuccessfully = await _menuRepository.AddDeviceMenu(addedDevice);
        if (menuAddedSuccessfully)
        {
            // 菜单也添加成功，通知 UI 更新
            MessageHelper.SendLoadMessage(LoadTypes.Menu);
            MessageHelper.SendLoadMessage(LoadTypes.Devices);
        }
        else
        {
            // 菜单添加失败，通知用户并记录日志
            HandleOperationFailure("设备", "添加菜单", addedDevice.Name);
            // 考虑：如果菜单添加失败，是否需要回滚设备添加？
            // 例如：await _deviceRepository.Delete(addedDevice.Id);
        }
    }
    catch (Exception e)
    {
        // 捕获并记录所有未预期的异常
        _logger.LogError(e, "在添加设备过程中发生未预期错误。");
        NotificationHelper.ShowMessage($"添加设备失败：{e.Message}", NotificationType.Error);
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
                // 更新菜单
                var res = await _deviceRepository.Edit(editDievce);
                var menu = DataServicesHelper.FindMenusForDevice(editDievce, _dataServices.MenuTrees);
                if (menu != null)
                     await _menuRepository.Edit(menu);

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
                // 删除设备
                await _deviceRepository.DeleteById(SelectedDevice.Id);
                // 删除菜单
                var menu = DataServicesHelper.FindMenusForDevice(SelectedDevice, _dataServices.MenuTrees);
                if (menu != null)
                    await _menuRepository.DeleteMenu(menu);
                
                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
                NotificationHelper.ShowMessage($"删除设备成功,设备名：{SelectedDevice.Name}", NotificationType.Success);
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"删除设备的过程中发生错误：{e.Message}", NotificationType.Error);
            _logger.LogError($"删除设备的过程中发生错误：{e}");
        }
    }




}