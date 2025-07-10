using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PMSWPF.Data;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

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
        IDialogService dialogService, DataServices dataServices
    )
    {
        _deviceRepository = new DeviceRepository();
        _varTableRepository = new VarTableRepository();
        _menuRepository = new MenuRepository();
        
        _dialogService = dialogService;
        _dataServices = dataServices;

        MessageHelper.SendLoadMessage(LoadTypes.Devices);
        _dataServices.OnDeviceListChanged += (sender, devices) =>
        {
            Devices = new ObservableCollection<Device>(devices);
        };
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

            await _deviceRepository.Add(device);
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
                await _deviceRepository.Delete(SelectedDevice ,_dataServices.MenuTrees);

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
            NotificationHelper.ShowError($"编辑设备的过程中发生错误：{e.Message}", e);
        }
    }
}