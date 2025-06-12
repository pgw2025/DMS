using PMSWPF.Models;

namespace PMSWPF.Services;

public interface IDeviceDialogService
{
    Task<Device> ShowAddDeviceDialog();
}