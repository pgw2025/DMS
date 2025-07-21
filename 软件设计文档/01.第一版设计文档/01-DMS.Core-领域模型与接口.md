# 软件开发文档 - DMS.Core 领域模型与接口

本文档详细阐述了 `DMS.Core` 项目的设计，它是整个系统的基石，包含所有业务实体和核心接口的定义。`DMS.Core` 不包含任何具体实现，确保了业务规则的独立性和可移植性。

## 1. 目录结构

```
DMS.Core/
├── Enums/
│   ├── PollLevelType.cs
│   ├── ProtocolType.cs
│   └── SignalType.cs
├── Models/
│   ├── Device.cs
│   ├── MqttServer.cs
│   ├── Variable.cs
│   ├── VariableHistory.cs
│   ├── VariableMqttAlias.cs
│   ├── VariableTable.cs
│   ├── MenuBean.cs
│   └── User.cs
├── Interfaces/
│   ├── IRepositoryManager.cs
│   ├── IBaseRepository.cs
│   ├── IDeviceRepository.cs
│   ├── IMqttServerRepository.cs
│   ├── IVariableHistoryRepository.cs
│   ├── IVariableMqttAliasRepository.cs
│   ├── IVariableRepository.cs
│   ├── IVariableTableRepository.cs
│   ├── IMenuRepository.cs
│   └── IUserRepository.cs
└── DMS.Core.csproj
```

## 2. 核心枚举 (`Enums/`)

使用C#枚举来表示业务中固定的、有限的分类，如协议类型、信号类型、轮询级别。这提供了类型安全和代码可读性。

### `PollLevelType.cs`

```csharp
// 文件: DMS.Core/Enums/PollLevelType.cs
/// <summary>
/// 定义了变量的轮询级别，决定了其读取频率。
/// </summary>
public enum PollLevelType
{
    /// <summary>
    /// 不进行轮询。
    /// </summary>
    Off,

    /// <summary>
    /// 高频轮询（例如：200ms）。
    /// </summary>
    High,

    /// <summary>
    /// 中频轮询（例如：1000ms）。
    /// </summary>
    Medium,

    /// <summary>
    /// 低频轮询（例如：5000ms）。
    /// </summary>
    Low
}
```

### `ProtocolType.cs`

```csharp
// 文件: DMS.Core/Enums/ProtocolType.cs
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

### `SignalType.cs`

```csharp
// 文件: DMS.Core/Enums/SignalType.cs
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

## 3. 领域模型 (`Models/`)

领域模型是业务核心的C#类表示。它们是贫血模型，主要包含数据属性，行为逻辑则由应用服务和领域服务处理。模型之间通过导航属性建立关系，反映业务实体间的关联。

### `Device.cs`

```csharp
// 文件: DMS.Core/Models/Device.cs
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
    /// S7 PLC的机架号。
    /// </summary>
    public int Rack { get; set; }

    /// <summary>
    /// S7 PLC的槽号。
    /// </summary>
    public int Slot { get; set; }

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

### `VariableTable.cs`

```csharp
// 文件: DMS.Core/Models/VariableTable.cs
namespace DMS.Core.Models;

/// <summary>
/// 组织和管理一组相关的变量。
/// </summary>
public class VariableTable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } // 是否启用
    public int DeviceId { get; set; }
    public Device Device { get; set; }
    public List<Variable> Variables { get; set; } = new();
}
```

### `Variable.cs`

```csharp
// 文件: DMS.Core/Models/Variable.cs
/// <summary>
/// 核心数据点，代表从设备读取的单个值。
/// </summary>
public class Variable
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 变量名。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 在设备中的地址 (例如: DB1.DBD0, M100.0)。
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 变量的数据类型。
    /// </summary>
    public SignalType DataType { get; set; }

    /// <summary>
    /// 变量的轮询级别，决定了其读取频率。
    /// </summary>
    public PollLevelType PollLevel { get; set; }

    /// <summary>
    /// 指示此变量是否处于激活状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 所属变量表的ID。
    /// </summary>
    public int VariableTableId { get; set; }

    /// <summary>
    /// 所属变量表的导航属性。
    /// </summary>
    public VariableTable VariableTable { get; set; }

    /// <summary>
    /// 此变量的所有MQTT发布别名关联。一个变量可以关联多个MQTT服务器，每个关联可以有独立的别名。
    /// </summary>
    public List<VariableMqttAlias> MqttAliases { get; set; } = new();

    /// <summary>
    /// 存储从设备读取到的最新值。此属性不应持久化到数据库，仅用于运行时。
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped] // 标记此属性不映射到数据库
    public object DataValue { get; set; }
}
```

### `MqttServer.cs`

```csharp
// 文件: DMS.Core/Models/MqttServer.cs
namespace DMS.Core.Models;

/// <summary>
/// 代表一个MQTT Broker的配置。
/// </summary>
public class MqttServer
{
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string BrokerAddress { get; set; } // Broker地址
    public int Port { get; set; } // 端口
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; } // 是否启用

    /// <summary>
    /// 与此服务器关联的所有变量别名。通过此集合可以反向查找关联的变量。
    /// </summary>
    public List<VariableMqttAlias> VariableAliases { get; set; } = new();
}
```

### `VariableHistory.cs`

```csharp
// 文件: DMS.Core/Models/VariableHistory.cs
namespace DMS.Core.Models;

/// <summary>
/// 用于存储变量值的变化记录。
/// </summary>
public class VariableHistory
{
    public long Id { get; set; }
    public int VariableId { get; set; }
    public string Value { get; set; } // 以字符串形式存储，便于通用性
    public DateTime Timestamp { get; set; }
}
```

### `VariableMqttAlias.cs`

```csharp
// 文件: DMS.Core/Models/VariableMqttAlias.cs
/// <summary>
/// 领域模型：代表一个变量到一个MQTT服务器的特定关联，包含专属别名。
/// 这是一个关联实体，用于解决多对多关系中需要额外属性（别名）的问题。
/// </summary>
public class VariableMqttAlias
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联的变量ID。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 关联的MQTT服务器ID。
    /// </summary>
    public int MqttServerId { get; set; }

    /// <summary>
    /// 针对此特定[变量-服务器]连接的发布别名。此别名将用于构建MQTT Topic。
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// 关联的变量导航属性。
    /// </summary>
    public Variable Variable { get; set; }

    /// <summary>
    /// 关联的MQTT服务器导航属性。
    /// </summary>
    public MqttServer MqttServer { get; set; }
}
```

### `MenuBean.cs`

```csharp
// 文件: DMS.Core/Models/MenuBean.cs
namespace DMS.Core.Models;

/// <summary>
/// 领域模型：代表一个菜单项。
/// </summary>
public class MenuBean
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Header { get; set; }
    public string Icon { get; set; }
    public string TargetViewKey { get; set; }
    public string NavigationParameter { get; set; }
    public int DisplayOrder { get; set; }
}
```

### `User.cs`

```csharp
// 文件: DMS.Core/Models/User.cs
namespace DMS.Core.Models;

/// <summary>
/// 领域模型：代表一个用户。
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; } // 存储密码哈希值
    public string Role { get; set; } // 用户角色，例如 "Admin", "Operator"
    public bool IsActive { get; set; }
}
```

## 4. 核心接口 (`Interfaces/`)

### 4.1. `IRepositoryManager.cs` (工作单元模式)

定义了一个仓储管理器，它使用工作单元模式来组合多个仓储操作，以确保事务的原子性。它作为所有仓储的统一入口，并管理数据库事务。

```csharp
// 文件: DMS.Core/Interfaces/IRepositoryManager.cs
namespace DMS.Core.Interfaces;

/// <summary>
/// 定义了一个仓储管理器，它使用工作单元模式来组合多个仓储操作，以确保事务的原子性。
/// 实现了IDisposable，以确保数据库连接等资源能被正确释放。
/// </summary>
public interface IRepositoryManager : IDisposable
{
    /// <summary>
    /// 获取设备仓储的实例。
    /// 所有通过此管理器获取的仓储都共享同一个数据库上下文和事务。
    /// </summary>
    IDeviceRepository Devices { get; }

    /// <summary>
    /// 获取变量表仓储的实例。
    /// </summary>
    IVariableTableRepository VariableTables { get; }

    /// <summary>
    /// 获取变量仓储的实例。
    /// </summary>
    IVariableRepository Variables { get; }

    /// <summary>
    /// 获取MQTT服务器仓储的实例。
    /// </summary>
    IMqttServerRepository MqttServers { get; }

    /// <summary>
    /// 获取变量MQTT别名仓储的实例。
    /// </summary>
    IVariableMqttAliasRepository VariableMqttAliases { get; }

    /// <summary>
    /// 获取菜单仓储的实例。
    /// </summary>
    IMenuRepository Menus { get; }

    /// <summary>
    /// 获取变量历史仓储的实例。
    /// </summary>
    IVariableHistoryRepository VariableHistories { get; }

    /// <summary>
    /// 获取用户仓储的实例。
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// 开始一个新的数据库事务。
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// 异步提交当前事务中的所有变更。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    Task CommitAsync();

    /// <summary>
    /// 异步回滚当前事务中的所有变更。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    Task RollbackAsync();
}
```

### 4.2. 仓储接口 (`IBaseRepository.cs`, `IDeviceRepository.cs` 等)

采用仓储（Repository）模式，为每个聚合根（或主要实体）定义一个数据访问接口。这些接口定义了对领域对象集合的操作，隐藏了底层数据存储的细节。

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
    Task<List<Device>> GetActiveDevicesWithDetailsAsync(ProtocolType protocol);

    /// <summary>
    /// 异步根据设备ID获取设备及其所有详细信息（变量表、变量、MQTT别名等）。
    /// </summary>
    /// <param name="deviceId">设备ID。</param>
    /// <returns>包含详细信息的设备对象。</returns>
    Task<Device> GetDeviceWithDetailsAsync(int deviceId);
}
```

#### `IVariableRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IVariableRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableRepository : IBaseRepository<Variable>
{
    /// <summary>
    /// 异步获取一个变量及其关联的所有MQTT别名和对应的MQTT服务器信息。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <returns>包含别名和服务器信息的变量对象。</returns>
    Task<Variable> GetVariableWithMqttAliasesAsync(int variableId);
}
```

#### `IMqttServerRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IMqttServerRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IMqttServerRepository : IBaseRepository<MqttServer>
{
    /// <summary>
    /// 异步获取一个MQTT服务器及其关联的所有变量别名。
    /// </summary>
    /// <param name="serverId">MQTT服务器ID。</param>
    /// <returns>包含变量别名信息的MQTT服务器对象。</returns>
    Task<MqttServer> GetMqttServerWithVariableAliasesAsync(int serverId);
}
```

#### `IVariableTableRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IVariableTableRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableTableRepository : IBaseRepository<VariableTable>
{
    // 可以添加特定于VariableTable的查询方法
}
```

#### `IVariableHistoryRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IVariableHistoryRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableHistoryRepository : IBaseRepository<VariableHistory>
{
    // 可以添加特定于VariableHistory的查询方法
}
```

#### `IVariableMqttAliasRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IVariableMqttAliasRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableMqttAliasRepository : IBaseRepository<VariableMqttAlias>
{
    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联，并加载关联的MQTT服务器信息。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <returns>指定变量的所有MQTT别名关联列表。</returns>
    Task<List<VariableMqttAlias>> GetAliasesForVariableAsync(int variableId);

    /// <summary>
    /// 异步根据变量ID和MQTT服务器ID获取特定的MQTT别名关联。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <param name="mqttServerId">MQTT服务器ID。</param>
    /// <returns>匹配的VariableMqttAlias对象，如果不存在则为null。</returns>
    Task<VariableMqttAlias> GetByVariableAndServerAsync(int variableId, int mqttServerId);
}
```

#### `IMenuRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IMenuRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IMenuRepository : IBaseRepository<MenuBean>
{
    // 可以添加特定于菜单的查询方法，例如获取所有菜单项
}
```

#### `IUserRepository.cs`

```csharp
// 文件: DMS.Core/Interfaces/IUserRepository.cs
using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    /// 异步根据用户名获取用户。
    /// </summary>
    /// <param name="username">用户名。</param>
    /// <returns>用户对象，如果不存在则为null。</returns>
    Task<User> GetByUsernameAsync(string username);
}
```
