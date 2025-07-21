# 软件开发文档 - 03. DMS.Infrastructure 详细设计

`DMS.Infrastructure` 层是所有外部技术和服务的具体实现地。它实现了 `DMS.Core` 定义的接口，为 `DMS.Application` 层提供数据和功能支持。

## 1. 目录结构

```
DMS.Infrastructure/
├── Data/
│   └── SqlSugarDbContext.cs
├── Entities/
│   ├── DbDevice.cs
│   ├── DbMqttServer.cs
│   ├── DbVariable.cs
│   └── ... (所有数据库表对应的实体)
├── Repositories/
│   ├── BaseRepository.cs
│   ├── DeviceRepository.cs
│   └── ... (所有仓储接口的实现)
├── Services/
│   ├── Communication/
│   │   ├── S7CommunicationService.cs
│   │   └── MqttPublishService.cs
│   ├── Processing/
│   │   ├── ChangeDetectionProcessor.cs
│   │   ├── HistoryStorageProcessor.cs
│   │   └── MqttPublishProcessor.cs
│   └── DatabaseInitializerService.cs
└── DMS.Infrastructure.csproj
```

## 2. 文件详解

### 2.1. 数据库实体 (`Entities/`)

#### `DbDevice.cs`

```csharp
// 文件: DMS.Infrastructure/Entities/DbDevice.cs
using SqlSugar;

namespace DMS.Infrastructure.Entities;

[SugarTable("Devices")]
public class DbDevice
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public bool IsActive { get; set; }
}
```
... (其他实体 `DbVariableTable`, `DbVariable`, `DbMqttServer`, `DbVariableHistory`, `DbVariableMqttMap` 结构类似) ...

### 2.2. 仓储实现 (`Repositories/`)

#### `BaseRepository.cs`

```csharp
// 文件: DMS.Infrastructure/Repositories/BaseRepository.cs
// 注意：这里需要一个从领域模型到数据库实体的映射，实际项目中通常使用AutoMapper的ProjectTo或手动映射。
// 为简化示例，此处仅做基础实现。
```

#### `DeviceRepository.cs`

```csharp
// 文件: DMS.Infrastructure/Repositories/DeviceRepository.cs
using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using SqlSugar;

public class DeviceRepository : IDeviceRepository
{
    private readonly ISqlSugarClient _db;
    private readonly IMapper _mapper;

    public DeviceRepository(ISqlSugarClient db, IMapper mapper) { /* ... */ }

    public async Task<List<Device>> GetActiveDevicesWithDetailsAsync()
    {
        var dbResult = await _db.Queryable<DbDevice>()
            .Where(d => d.IsActive)
            .ToListAsync();
        // 此处需要手动或通过AutoMapper加载关联的变量表和变量
        return _mapper.Map<List<Device>>(dbResult);
    }
    // ... 其他方法的实现
}
```

### 2.3. 外部服务 (`Services/`)

#### `S7CommunicationService.cs`

```csharp
// 文件: DMS.Infrastructure/Services/Communication/S7CommunicationService.cs
using S7.Net;
using System.Collections.Concurrent;

public class S7CommunicationService : IS7CommunicationService
{
    private readonly ConcurrentDictionary<int, Plc> _clients = new();

    public async Task<object> ReadVariableAsync(Device device, Variable variable)
    {
        if (!_clients.TryGetValue(device.Id, out var plc))
        {
            plc = new Plc(CpuType.S71500, device.IpAddress, 0, 1);
            await plc.OpenAsync();
            _clients[device.Id] = plc;
        }
        return await plc.ReadAsync(variable.Address);
    }
    // ... 其他连接、断开等管理方法
}
```

#### `MqttPublishService.cs`

```csharp
// 文件: DMS.Infrastructure/Services/Communication/MqttPublishService.cs
using MQTTnet;
using MQTTnet.Client;

public class MqttPublishService : IMqttPublishService
{
    private readonly IMqttClient _mqttClient;

    public MqttPublishService() {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
    }

    public async Task PublishAsync(MqttServer server, string topic, string payload)
    {
        if (!_mqttClient.IsConnected)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(server.BrokerAddress, server.Port)
                .WithCredentials(server.Username, server.Password)
                .Build();
            await _mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, CancellationToken.None);
    }
}
```

### 2.4. 数据处理器 (`Services/Processing/`)

(此处包含 `ChangeDetectionProcessor`, `HistoryStorageProcessor`, `MqttPublishProcessor` 的完整实现，如之前文档所示，但会更加细化，并注入 `ILogger`)
