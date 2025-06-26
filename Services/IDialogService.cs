using PMSWPF.Models;

namespace PMSWPF.Services;

public interface IDialogService
{
    Task<Device> ShowAddDeviceDialog();

    void ShowMessageDialog(string title, string message);
}