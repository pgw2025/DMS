using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces;

public interface IDeviceDataService
{
    List<Device> Devices { get; }
    event Action<List<Device>> OnDeviceListChanged;
    event Action<Device, bool> OnDeviceIsActiveChanged;
    Task InitializeAsync();
}