using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Data.Repositories;
using DMS.Enums;
using DMS.Helper;
using DMS.Models;
using DMS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DMS.Data;

namespace DMS.ViewModels;

/// <summary>
/// 设备管理视图模型，负责设备的增删改查操作。
/// </summary>
public partial class DevicesViewModel : ViewModelBase
{
    private readonly DataServices _dataServices;
    private readonly IDialogService _dialogService;
    private readonly DeviceRepository _deviceRepository;
    
    private readonly MenuRepository _menuRepository;
    private readonly VarTableRepository _varTableRepository;

    /// <summary>
    /// 设备列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Device> _devices;

    
    /// <summary>
    /// 当前选中的设备。
    /// </summary>
    [ObservableProperty]
    private Device _selectedDevice;

    /// <summary>
    /// 初始化 <see cref="DevicesViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="logger">日志记录器。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <param name="dataServices">数据服务。</param>
    public DevicesViewModel(
        IDialogService dialogService, DataServices dataServices,DeviceRepository deviceRepository,VarTableRepository varTableRepository,MenuRepository menuRepository
    )
    {
        _deviceRepository = deviceRepository;
        _varTableRepository = varTableRepository;
        _menuRepository = menuRepository;
        
        _dialogService = dialogService;
        _dataServices = dataServices;

        _dataServices.OnDeviceListChanged += (devices) =>
        {
            Devices = new ObservableCollection<Device>(devices);
        };
    }

    public override void OnLoaded()
    {
        if (_dataServices.Devices!=null && _dataServices.Devices.Count>0)
        {
            Devices=new ObservableCollection<Device>(_dataServices.Devices);
            foreach (var device in Devices)
            {
                device.OnDeviceIsActiveChanged += HandleDeviceIsActiveChanged;
                
            }
        }
    }

    public override Task<bool> OnExitAsync()
    {
        if (_dataServices.Devices!=null && _dataServices.Devices.Count>0)
        {
            foreach (var device in Devices)
            {
                device.OnDeviceIsActiveChanged -= HandleDeviceIsActiveChanged;
            }
        }

        return Task.FromResult(true);
    }

    private async void HandleDeviceIsActiveChanged(Device device, bool isActive)
    {
        try
        {
            await _deviceRepository.UpdateAsync(device);
            NotificationHelper.ShowSuccess($"设备 {device.Name} 的激活状态已更新。");
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"更新设备 {device.Name} 激活状态失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 添加设备命令。
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
                NlogHelper.Info("用户取消了添加设备操作。");
                return;
            }

            if (device.ProtocolType == ProtocolType.OpcUA)
                device.OpcUaEndpointUrl = $"opc.tcp://{device.Ip}:{device.Prot}";

            await _deviceRepository.AddAsync(device);
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"添加设备的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 删除设备命令。
    /// </summary>
    [RelayCommand]
    public async void DeleteDevice()
    {
        
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowError("你没有选择任何设备，请选择设备后再点击删除设备");
                return;
            }

            string msg = $"确认要删除设备名为:{SelectedDevice.Name}";
            var isDel = await _dialogService.ShowConfrimeDialog("删除设备", msg, "删除设备");
            if (isDel)
            {
                
                // 删除设备
                await _deviceRepository.DeleteAsync(SelectedDevice ,_dataServices.MenuTrees);

                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
                NotificationHelper.ShowSuccess($"删除设备成功,设备名：{SelectedDevice.Name}");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除设备的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 编辑设备命令。
    /// </summary>
    [RelayCommand]
    public async void EditDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowError("你没有选择任何设备，请选择设备后再点击编辑设备");
                return;
            }

            var editDievce = await _dialogService.ShowEditDeviceDialog(SelectedDevice);
            if (editDievce != null)
            {
                // 更新菜单
                var res = await _deviceRepository.UpdateAsync(editDievce);
                var menu = DataServicesHelper.FindMenusForDevice(editDievce, _dataServices.MenuTrees);
                if (menu != null)
                    await _menuRepository.UpdateAsync(menu);

                MessageHelper.SendLoadMessage(LoadTypes.Menu);
                MessageHelper.SendLoadMessage(LoadTypes.Devices);
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"编辑设备的过程中发生错误：{e.Message}", e);
        }
    }
    
    [RelayCommand]
    public void NavigateToDetail()
    {
        if (SelectedDevice == null) return;
        var deviceDetailVm = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
        deviceDetailVm.CurrentDevice = SelectedDevice;
        MessageHelper.SendNavgatorMessage(deviceDetailVm);
    }
}