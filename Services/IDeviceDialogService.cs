using PMSWPF.Models;

namespace PMSWPF.Services;

public interface IDeviceDialogService
{
    Task<Device> ShowAddDeviceDialog();

    void ShowMessageDialog(string title, string message);
}