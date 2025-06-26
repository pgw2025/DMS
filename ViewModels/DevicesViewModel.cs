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

    [ObservableProperty] private ObservableCollection<Device> _devices;
    private readonly MenuRepository _menuRepository;

    public DevicesViewModel(
        ILogger<DevicesViewModel> logger, IDialogService dialogService
    )
    {
        _deviceRepository = new DeviceRepository();
        _menuRepository = new MenuRepository();
        _logger = logger;
        _dialogService = dialogService;
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _deviceRepository.GetAll();
        Devices = new ObservableCollection<Device>(ds);
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
                    // 添加菜单项
                    MenuBean deviceMenu = new MenuBean()
                    {
                        Name = device.Name,
                        Icon = SegoeFluentIcons.Devices4.Glyph,
                    };
                    var rows = await _menuRepository.AddDeviceMenu(deviceMenu);
                    if (rows > 0)
                    {
                        WeakReferenceMessenger.Default.Send<UpdateMenuMessage>(new UpdateMenuMessage(2));
                    }
                }
                else
                {
                    var msg = $"添加设备失败：{device.Name}";
                    _logger.LogInformation(msg);
                }
            }

            // OpenDialogMessage dialog = new OpenDialogMessage();
            //
            // var res=WeakReferenceMessenger.Default.Send<OpenDialogMessage>(dialog);

            Console.WriteLine("");

            // device = await _deviceDialogService.ShowAddDeviceDialog();
            // if (device != null)
            // {
            //     var isOk = await _deviceRepository.Add(device);
            //     if (isOk)
            //     {
            //         // 添加菜单项
            //         MenuBean deviceMenu = new MenuBean()
            //             { Name = device.Name, Icon = SegoeFluentIcons.Devices4.Glyph, ParentId = 2 };
            //         MenuRepository mre = new MenuRepository();
            //         mre.AddMenu(deviceMenu);
            //         
            //         // MessageBox.Show("Device added successfully");
            //         await OnLoadedAsync();
            //         var msg = $"设备添加成功：{device.Name}";
            //         _logger.LogInformation(msg);
            //         NotificationHelper.ShowMessage(msg, NotificationType.Success);
            //     }
            // }
        }
        catch (DbExistException e)
        {
            var msg = $"设备添加失败：名称为{device?.Name}的设备已经存在。请更换是被名称";
            _logger.LogError(msg);
            NotificationHelper.ShowMessage(msg, NotificationType.Error);
        }
        catch (Exception e)
        {
            var msg = $"添加设备的过程中发生错误：{e.Message}";
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