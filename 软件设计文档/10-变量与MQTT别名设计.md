# 软件开发文档 - 10. 变量与MQTT别名设计

本文档详细阐述了为满足“一个变量在关联不同MQTT服务器时可以有不同别名”这一需求而设计的“关联实体”架构方案。

## 1. 设计挑战与解决方案

**挑战**：一个简单的多对多映射表无法存储“关系”本身的数据（即“别名”）。我们需要一个能将别名属性牢固地绑定在“变量-服务器”这一特定连接上的机制。

**解决方案**：引入一个功能完整的“**关联实体**”（Association Entity），我们将其命名为 `VariableMqttAlias`。这个实体不仅连接了 `Variable` 和 `MqttServer`，它自身还携带了数据（`Alias` 属性），完美地解决了问题。

## 2. 数据库与核心模型设计

我们将用新的 `VariableMqttAlias` 实体来取代之前简单的多对多映射表。

### 2.1. 数据库实体 (`DMS.Infrastructure`)

```csharp
// 文件: DMS.Infrastructure/Entities/DbVariableMqttAlias.cs
using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 关联实体：连接变量和MQTT服务器，并存储该连接专属的别名。
/// </summary>
[SugarTable("VariableMqttAliases")]
public class DbVariableMqttAlias
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 外键，指向 Variables 表。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 外键，指向 MqttServers 表。
    /// </summary>
    public int MqttServerId { get; set; }

    /// <summary>
    /// 针对此特定[变量-服务器]连接的发布别名。
    /// </summary>
    [SugarColumn(Length = 200)]
    public string Alias { get; set; }
}
```

### 2.2. 领域模型 (`DMS.Core`)

`Variable` 和 `MqttServer` 的领域模型现在通过 `VariableMqttAlias` 间接关联。

```csharp
// 文件: DMS.Core/Models/VariableMqttAlias.cs (新增)
public class VariableMqttAlias
{
    public int Id { get; set; }
    public int VariableId { get; set; }
    public int MqttServerId { get; set; }
    public string Alias { get; set; }
    public Variable Variable { get; set; }
    public MqttServer MqttServer { get; set; }
}

// 文件: DMS.Core/Models/Variable.cs (修改)
public class Variable
{
    // ... 其他属性
    // 移除: public List<MqttServer> MqttServers { get; set; }
    public List<VariableMqttAlias> MqttAliases { get; set; } = new();
}

// 文件: DMS.Core/Models/MqttServer.cs (修改)
public class MqttServer
{
    // ... 其他属性
    // 移除: public List<Variable> Variables { get; set; }
    public List<VariableMqttAlias> VariableAliases { get; set; } = new();
}
```

## 3. 数据处理链更新 (`DMS.Infrastructure`)

`MqttPublishProcessor` 现在必须使用新的关联实体来获取别名和目标服务器。

```csharp
// 文件: DMS.Infrastructure/Services/Processors/MqttPublishProcessor.cs (修改)
public class MqttPublishProcessor : VariableProcessorBase
{
    private readonly IMqttPublishService _mqttService;
    private readonly IRepositoryManager _repoManager;

    public MqttPublishProcessor(IMqttPublishService mqttService, IRepositoryManager repoManager) { /*...*/ }

    protected override async Task HandleAsync(VariableContext context)
    {
        if (!context.IsValueChanged) return;

        // 1. 获取变量及其完整的别名关联列表
        var variableWithAliases = await _repoManager.Variables.GetWithAliasesAsync(context.Variable.Id);
        if (variableWithAliases?.MqttAliases == null) return;

        // 2. 遍历每一个“别名关联”
        foreach (var aliasInfo in variableWithAliases.MqttAliases)
        {
            var targetServer = aliasInfo.MqttServer;
            if (targetServer == null || !targetServer.IsActive) continue;

            // 3. 使用别名构建Topic
            var topic = $"devices/{variableWithAliases.VariableTable.Device.Name}/{aliasInfo.Alias}";
            var payload = System.Text.Json.JsonSerializer.Serialize(new { value = context.CurrentValue, timestamp = context.Timestamp });

            await _mqttService.PublishAsync(targetServer, topic, payload);
        }
    }
}
```

## 4. 应用层支持 (`DMS.Application`)

为了让UI能够管理这些别名，应用层需要提供相应的CRUD服务。

### 4.1. 新DTO

```csharp
// 文件: DMS.Application/DTOs/VariableMqttAliasDto.cs
public class VariableMqttAliasDto
{
    public int Id { get; set; }
    public int VariableId { get; set; }
    public int MqttServerId { get; set; }
    public string MqttServerName { get; set; }
    public string Alias { get; set; }
}
```

### 4.2. 应用服务接口扩展

`IVariableAppService` 或一个新的 `IMqttAliasAppService` 需要提供管理别名的方法。

```csharp
// 示例接口
public interface IMqttAliasAppService
{
    Task<List<VariableMqttAliasDto>> GetAliasesForVariableAsync(int variableId);
    Task AssignAliasAsync(int variableId, int mqttServerId, string alias);
    Task UpdateAliasAsync(int aliasId, string newAlias);
    Task RemoveAliasAsync(int aliasId);
}
```

### 4.3. 应用服务实现

服务实现将使用 `IRepositoryManager` 来操作 `VariableMqttAlias` 仓储。

```csharp
// 示例实现
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    public async Task AssignAliasAsync(int variableId, int mqttServerId, string alias)
    {
        var newAlias = new VariableMqttAlias
        {
            VariableId = variableId,
            MqttServerId = mqttServerId,
            Alias = alias
        };
        await _repoManager.VariableMqttAliases.AddAsync(newAlias);
        await _repoManager.CommitAsync();
    }
    // ... 其他方法的实现
}
```

**注意**：此设计要求为 `VariableMqttAlias` 创建一个专属的仓储接口 (`IVariableMqttAliasRepository`) 和实现，并将其添加到 `IRepositoryManager` 中。
