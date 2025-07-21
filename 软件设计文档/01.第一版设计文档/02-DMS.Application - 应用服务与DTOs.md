# 软件开发文档 - DMS.Application - 应用服务与DTOs

本文档详细阐述了 `DMS.Application` 层的设计。该层作为业务逻辑的协调者，负责处理用例、数据转换，并作为表现层与核心层的桥梁。

## 1. 目录结构

```
DMS.Application/
├── DTOs/
│   ├── DeviceDto.cs
│   ├── CreateDeviceDto.cs
│   ├── UpdateDeviceDto.cs
│   ├── VariableDto.cs
│   ├── VariableTableDto.cs
│   ├── MqttServerDto.cs
│   ├── VariableMqttAliasDto.cs
│   └── CreateDeviceWithDetailsDto.cs
├── Interfaces/
│   ├── IDeviceAppService.cs
│   ├── IMqttAppService.cs
│   ├── IVariableAppService.cs
│   ├── IMqttAliasAppService.cs
│   └── IMenuService.cs
├── Services/
│   ├── DeviceAppService.cs
│   ├── MqttAppService.cs
│   ├── VariableAppService.cs
│   ├── MqttAliasAppService.cs
│   └── MenuService.cs
├── Profiles/
│   └── MappingProfile.cs
└── DMS.Application.csproj
```

## 2. 应用服务设计

### 2.1. 设计思路与考量

*   **职责**：应用服务（Application Services）是应用程序的用例（Use Case）实现者。它们负责编排领域模型和仓储来完成特定的业务操作，处理事务、授权、日志等应用级关注点。
*   **依赖**：应用服务依赖于 `DMS.Core` 中定义的接口（如 `IRepositoryManager`），而不直接依赖 `DMS.Infrastructure` 的具体实现，遵循依赖倒置原则。
*   **事务管理**：通过注入 `IRepositoryManager` 来管理事务，确保业务操作的原子性。

### 2.2. 设计优势

*   **业务流程清晰**：每个应用服务方法对应一个具体的业务用例，代码结构清晰，易于理解业务流程。
*   **解耦**：将业务逻辑与UI层和数据访问层解耦，提高了代码的可维护性和可测试性。
*   **事务边界明确**：应用服务是定义事务边界的理想位置，确保业务操作的完整性。
*   **可重用性**：应用服务可以被不同的客户端（如WPF UI、Web API等）复用。

### 2.3. 设计劣势/权衡

*   **代码量增加**：相比于直接在UI层或仓储层编写业务逻辑，应用服务模式会增加一些代码量和抽象层级。
*   **贫血应用服务**：如果应用服务仅仅是简单地调用仓储方法，而没有包含任何业务逻辑，则可能退化为“贫血应用服务”，失去了其应有的价值。

### 2.4. 示例：`IDeviceAppService.cs`

```csharp
// 文件: DMS.Application/Interfaces/IDeviceAppService.cs
using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义设备管理相关的应用服务操作。
/// </summary>
public interface IDeviceAppService
{
    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    Task<DeviceDto> GetDeviceByIdAsync(int id);

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    Task<List<DeviceDto>> GetAllDevicesAsync();

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    /// <param name="dto">包含设备、变量表和菜单信息的DTO。</param>
    /// <returns>新创建设备的ID。</returns>
    Task<int> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    Task UpdateDeviceAsync(UpdateDeviceDto deviceDto);

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    Task DeleteDeviceAsync(int id);

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    Task ToggleDeviceActiveStateAsync(int id);

    /// <summary>
    /// 异步获取指定协议类型的设备列表。
    /// </summary>
    Task<List<DeviceDto>> GetDevicesByProtocolAsync(ProtocolType protocol);
}
```

### 2.5. 示例：`DeviceAppService.cs`

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
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    public DeviceAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<int> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        try
        {
            _repoManager.BeginTransaction();

            var device = _mapper.Map<Device>(dto.Device);
            device.IsActive = true; // 默认激活
            await _repoManager.Devices.AddAsync(device);

            // 假设 CreateDeviceWithDetailsDto 包含了变量表和菜单信息
            if (dto.VariableTable != null)
            {
                var variableTable = _mapper.Map<VariableTable>(dto.VariableTable);
                variableTable.DeviceId = device.Id; // 关联新设备ID
                await _repoManager.VariableTables.AddAsync(variableTable);
            }

            // 假设有菜单服务或仓储
            // if (dto.Menu != null)
            // {
            //     var menu = _mapper.Map<Menu>(dto.Menu);
            //     menu.TargetId = device.Id;
            //     await _repoManager.Menus.AddAsync(menu);
            // }

            await _repoManager.CommitAsync();

            return device.Id;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            // 可以在此记录日志
            throw new ApplicationException("创建设备时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        var devices = await _repoManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }

    // ... 其他方法的实现
}
```

## 3. 数据传输对象 (DTOs)

### 3.1. 设计思路与考量

*   **职责**：DTOs (Data Transfer Objects) 是用于在不同层之间（特别是应用层和表现层之间）传输数据的简单对象。它们通常是扁平的，只包含数据属性，不包含行为。
*   **隔离**：DTOs 隔离了领域模型和UI层，避免了领域模型直接暴露给UI，从而保护了领域模型的完整性和不变性。
*   **定制化**：DTOs 可以根据UI的需求进行定制，例如，只包含UI需要显示的字段，或者将多个领域模型的字段组合成一个DTO。

### 3.2. 设计优势

*   **解耦**：UI层不直接依赖领域模型，领域模型的修改不会直接影响UI层。
*   **安全性**：避免了敏感数据或不必要的复杂领域逻辑暴露给UI层。
*   **性能优化**：可以只传输UI所需的数据，减少网络传输量（对于分布式系统）。
*   **简化UI绑定**：DTOs 通常是扁平的，更适合UI的数据绑定。

### 3.3. 设计劣势/权衡

*   **映射开销**：需要在领域模型和DTO之间进行映射，增加了代码量和运行时开销（尽管AutoMapper可以简化）。
*   **DTOs 膨胀**：如果每个UI视图都需要一个独立的DTO，可能导致DTO类的数量膨胀。

### 3.4. 示例：`DeviceDto.cs`

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

### 3.5. 示例：`CreateDeviceDto.cs`

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

### 3.6. 示例：`VariableMqttAliasDto.cs`

```csharp
// 文件: DMS.Application/DTOs/VariableMqttAliasDto.cs
namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示和管理变量与MQTT服务器关联别名的DTO。
/// </summary>
public class VariableMqttAliasDto
{
    public int Id { get; set; }
    public int VariableId { get; set; }
    public int MqttServerId { get; set; }
    public string MqttServerName { get; set; } // 用于UI显示关联的服务器名称
    public string Alias { get; set; }
}
```

### 3.7. 示例：`VariableDto.cs`

```csharp
// 文件: DMS.Application/DTOs/VariableDto.cs
using DMS.Core.Enums;
using System;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示变量基本信息的DTO。
/// </summary>
public class VariableDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public SignalType DataType { get; set; }
    public PollLevelType PollLevel { get; set; }
    public bool IsActive { get; set; }
    public int VariableTableId { get; set; }
    public string OpcUaNodeId { get; set; }
    public bool IsHistoryEnabled { get; set; }
    public double HistoryDeadband { get; set; }
    public bool IsAlarmEnabled { get; set; }
    public double AlarmMinValue { get; set; }
    public double AlarmMaxValue { get; set; }
    public double AlarmDeadband { get; set; }
    public ProtocolType Protocol { get; set; }
    public CSharpDataType CSharpDataType { get; set; }
    public string ConversionFormula { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsModified { get; set; }
}
```

### 3.8. 示例：`MqttServerDto.cs`

```csharp
// 文件: DMS.Application/DTOs/MqttServerDto.cs
using System;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示MQTT服务器配置信息的DTO。
/// </summary>
public class MqttServerDto
{
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string BrokerAddress { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public string SubscribeTopic { get; set; }
    public string PublishTopic { get; set; }
    public string ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public long ConnectionDuration { get; set; }
    public string MessageFormat { get; set; }
}
```

### 3.9. 示例：`VariableTableDto.cs`

```csharp
// 文件: DMS.Application/DTOs/VariableTableDto.cs
using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示变量表基本信息的DTO。
/// </summary>
public class VariableTableDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int DeviceId { get; set; }
    public ProtocolType Protocol { get; set; }
}
```

## 4. AutoMapper 配置

### 4.1. 设计思路与考量

*   **自动化映射**：使用 `AutoMapper` 库来自动化领域模型和DTO之间的对象映射，减少手动映射的重复代码。
*   **集中配置**：所有映射规则集中在一个 `MappingProfile` 类中进行配置。

### 4.2. 设计优势

*   **减少样板代码**：显著减少了手动进行属性赋值的重复代码。
*   **提高开发效率**：开发者可以专注于业务逻辑，而不是数据转换。
*   **可维护性**：映射规则集中管理，修改和审查方便。

### 4.3. 设计劣势/权衡

*   **隐式性**：映射过程是隐式的，对于不熟悉AutoMapper的开发者可能难以理解数据流向。
*   **调试难度**：当映射出现问题时，调试可能比手动映射更复杂。
*   **性能开销**：虽然AutoMapper经过优化，但在极端性能敏感的场景下，仍然可能比手动映射有轻微的性能开销。

### 4.4. 示例：`MappingProfile.cs`

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
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();

        // Variable 映射
        CreateMap<Variable, VariableDto>()
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()))
            .ForMember(dest => dest.CSharpDataType, opt => opt.MapFrom(src => src.CSharpDataType));

        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>().ReverseMap();

        // VariableMqttAlias 映射
        CreateMap<VariableMqttAlias, VariableMqttAliasDto>().ReverseMap();

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();
    }
}
```