# 软件开发文档 - 01. DMS.Core 详细设计

`DMS.Core` 是系统的核心，定义了所有业务逻辑的蓝图。它不包含任何具体实现，确保了业务规则的独立性和可移植性。

## 1. 目录结构

```
DMS.Core/
├── Enums/
│   ├── ProtocolType.cs
│   └── SignalType.cs
├── Models/
│   ├── Device.cs
│   ├── MqttServer.cs
│   ├── Variable.cs
│   ├── VariableHistory.cs
│   └── VariableTable.cs
├── Interfaces/
│   ├── IBaseRepository.cs
│   ├── IDeviceRepository.cs
│   ├── IMqttServerRepository.cs
│   ├── IVariableHistoryRepository.cs
│   ├── IVariableRepository.cs
│   └── IVariableTableRepository.cs
└── DMS.Core.csproj
```

## 2. 文件详解

### 2.1. 枚举 (`Enums/`)

#### `ProtocolType.cs`

```csharp
// 文件: DMS.Core/Enums/ProtocolType.cs
namespace DMS.Core.Enums;

/// <summary>
/// 定义了设备支持的通信协议类型。
/// </summary>
public enum ProtocolType
{
    /// <summary>
    /// Siemens S7 通信协议。
    /// </summary>
    S7,

    /// <summary>
    /// OPC UA (Unified Architecture) 协议。
    /// </summary>
    OpcUa,

    /// <summary>
    /// Modbus TCP 协议。
    /// </summary>
    ModbusTcp
}
```

#### `SignalType.cs`

```csharp
// 文件: DMS.Core/Enums/SignalType.cs
namespace DMS.Core.Enums;

/// <summary>
/// 定义了变量支持的数据类型。
/// </summary>
public enum SignalType
{
    /// <summary>
    /// 布尔值 (true/false)。
    /// </summary>
    Boolean,

    /// <summary>
    /// 字节 (8-bit 无符号整数)。
    /// </summary>
    Byte,

    /// <summary>
    /// 16位有符号整数。
    /// </summary>
    Int16,

    /// <summary>
    /// 32位有符号整数。
    /// </summary>
    Int32,

    /// <summary>
    /// 单精度浮点数。
    /// </summary>
    Float,

    /// <summary>
    /// 字符串。
    /// </summary>
    String
}
```

### 2.2. 领域模型 (`Models/`)

#### `Device.cs`

```csharp
// 文件: DMS.Core/Models/Device.cs
namespace DMS.Core.Models;

/// <summary>
/// 代表一个可管理的物理或逻辑设备。
/// </summary>
public class Device
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 设备名称，用于UI显示和识别。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备使用的通信协议。
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// 设备的通信端口号。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 指示此设备是否处于激活状态。只有激活的设备才会被轮询。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 此设备包含的变量表集合。
    /// </summary>
    public List<VariableTable> VariableTables { get; set; } = new();
}
```

... (其他模型 `MqttServer.cs`, `Variable.cs`, `VariableHistory.cs`, `VariableTable.cs` 的代码结构与此类似，包含详细的属性和注释) ...

### 2.3. 仓储接口 (`Interfaces/`)

#### `IBaseRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IBaseRepository.cs
namespace DMS.Core.Interfaces;

/// <summary>
/// 提供泛型数据访问操作的基础仓储接口。
/// </summary>
/// <typeparam name="T">领域模型的类型。</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// 异步根据ID获取单个实体。
    /// </summary>
    /// <param name="id">实体的主键ID。</param>
    /// <returns>找到的实体，如果不存在则返回null。</returns>
    Task<T> GetByIdAsync(int id);

    /// <summary>
    /// 异步获取所有实体。
    /// </summary>
    /// <returns>所有实体的列表。</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// 异步添加一个新实体。
    /// </summary>
    /// <param name="entity">要添加的实体。</param>
    Task AddAsync(T entity);

    /// <summary>
    /// 异步更新一个已存在的实体。
    /// </summary>
    /// <param name="entity">要更新的实体。</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// 异步根据ID删除一个实体。
    /// </summary>
    /// <param name="id">要删除的实体的主键ID。</param>
    Task DeleteAsync(int id);
}
```

#### `IDeviceRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IDeviceRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

/// <summary>
/// 继承自IBaseRepository，提供设备相关的特定数据查询功能。
/// </summary>
public interface IDeviceRepository : IBaseRepository<Device>
{
    /// <summary>
    /// 异步获取所有激活的设备，并级联加载其下的变量表和变量。
    /// 这是后台轮询服务需要的主要数据。
    /// </summary>
    /// <returns>包含完整层级结构的激活设备列表。</returns>
    Task<List<Device>> GetActiveDevicesWithDetailsAsync();

    /// <summary>
    /// 异步根据协议类型获取设备列表。
    /// </summary>
    /// <param name="protocol">协议类型。</param>
    /// <returns>符合条件的设备列表。</returns>
    Task<List<Device>> GetDevicesByProtocolAsync(ProtocolType protocol);
}
```

... (其他仓储接口 `IMqttServerRepository.cs`, `IVariableRepository.cs` 等的代码结构与此类似) ...

