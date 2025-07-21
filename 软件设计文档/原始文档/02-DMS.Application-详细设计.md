# 软件开发文档 - 02. DMS.Application 详细设计

`DMS.Application` 层是业务逻辑的执行者和协调者。它接收来自表现层的请求，使用 `DMS.Core` 的领域模型和接口来完成任务，并向上层返回DTOs。

## 1. 目录结构

```
DMS.Application/
├── DTOs/
│   ├── DeviceDto.cs
│   ├── MqttServerDto.cs
│   ├── VariableDto.cs
│   └── ... (其他增、删、改、查相关的DTO)
├── Interfaces/
│   ├── IDeviceAppService.cs
│   ├── IMqttAppService.cs
│   └── ... (其他应用服务接口)
├── Services/
│   ├── DeviceAppService.cs
│   ├── MqttAppService.cs
│   └── ... (其他应用服务实现)
├── Profiles/
│   └── MappingProfile.cs
└── DMS.Application.csproj
```

## 2. 文件详解

### 2.1. 数据传输对象 (`DTOs/`)

DTOs 是为了隔离领域模型和UI而设计的简单数据容器。

#### `DeviceDto.cs`

```csharp
// 文件: DMS.Application/DTOs/DeviceDto.cs
namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示设备基本信息的DTO。
/// </summary>
public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } // "在线", "离线", "连接中..."
}
```

#### `CreateDeviceDto.cs`

```csharp
// 文件: DMS.Application/DTOs/CreateDeviceDto.cs
using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于创建新设备时传输数据的DTO。
/// </summary>
public class CreateDeviceDto
{
    public string Name { get; set; }
    public ProtocolType Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
}
```

... (其他DTOs如 `UpdateDeviceDto`, `VariableTableDto`, `MqttServerDto` 等结构类似) ...

### 2.2. 应用服务接口 (`Interfaces/`)

#### `IDeviceAppService.cs`

```csharp
// 文件: DMS.Application/Interfaces/IDeviceAppService.cs
using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义设备管理相关的应用服务操作。
/// </summary>
public interface IDeviceAppService
{
    Task<DeviceDto> GetDeviceByIdAsync(int id);
    Task<List<DeviceDto>> GetAllDevicesAsync();
    Task<int> CreateDeviceAsync(CreateDeviceDto deviceDto);
    Task UpdateDeviceAsync(UpdateDeviceDto deviceDto);
    Task DeleteDeviceAsync(int id);
    Task ToggleDeviceActiveStateAsync(int id);
}
```

### 2.3. 应用服务实现 (`Services/`)

#### `DeviceAppService.cs`

```csharp
// 文件: DMS.Application/Services/DeviceAppService.cs
using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;

namespace DMS.Application.Services;

/// <summary>
/// 实现设备管理的应用服务。
/// </summary>
public class DeviceAppService : IDeviceAppService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public DeviceAppService(IDeviceRepository deviceRepository, IMapper mapper)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<int> CreateDeviceAsync(CreateDeviceDto deviceDto)
    {
        var device = _mapper.Map<Device>(deviceDto);
        device.IsActive = true; // 默认激活
        await _deviceRepository.AddAsync(device);
        return device.Id; // 返回新创建的ID
    }

    // ... 其他方法的完整实现
}
```

### 2.4. AutoMapper配置 (`Profiles/`)

#### `MappingProfile.cs`

```csharp
// 文件: DMS.Application/Profiles/MappingProfile.cs
using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Profiles;

/// <summary>
/// 配置AutoMapper的映射规则。
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Device 映射
        CreateMap<CreateDeviceDto, Device>();
        CreateMap<UpdateDeviceDto, Device>();
        CreateMap<Device, DeviceDto>()
            .ForMember(dest => dest.Protocol, opt => opt.MapFrom(src => src.Protocol.ToString()));

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>();

        // Variable 映射
        CreateMap<Variable, VariableDto>()
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()));

        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>();
    }
}
```
