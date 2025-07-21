# 软件开发文档 - MQTT别名关联设计

本文档详细阐述了为满足“一个变量在关联不同MQTT服务器时可以有不同别名”这一需求而设计的“关联实体”架构方案。

## 1. 设计方案：关联实体

### 1.1. 设计思路与考量

*   **挑战**：传统的简单多对多映射表（如 `Variable <-> MqttServer`）无法存储“关系”本身的属性。例如，当一个变量 `V1` 关联到 `MQTT_A` 时，其别名为 `Alias_A`；当 `V1` 关联到 `MQTT_B` 时，其别名为 `Alias_B`。这个别名 `Alias` 既不属于 `V1` 也不属于 `MQTT_A` 或 `MQTT_B`，它属于 `V1` 和 `MQTT_A` 之间的特定关联。
*   **解决方案**：引入一个功能完整的“**关联实体**”（Association Entity），我们将其命名为 `VariableMqttAlias`。这个实体作为 `Variable` 和 `MqttServer` 之间关系的载体，自身还携带了关系特有的属性（即 `Alias`）。

### 1.2. 设计优势

*   **数据完整性**：别名属性被牢固地绑定在“变量-服务器”的特定连接上，确保了数据的一致性和准确性。
*   **高度灵活性**：同一个变量可以为不同的MQTT服务器设置完全独立的别名，完美适应各种MQTT Broker对Topic命名命名规则的差异化要求。
*   **清晰的关注点分离**：数据模型清晰地反映了业务规则，使得数据处理链（特别是MQTT发布逻辑）能够明确地使用正确的别名。
*   **可管理性**：通过应用层提供的服务，UI可以方便地实现对这些别名关联的增、删、改、查操作。

### 1.3. 设计劣势/权衡

*   **复杂性增加**：相比于简单的多对多映射表，引入关联实体增加了额外的表、实体类和仓储，增加了代码量和理解成本。
*   **查询复杂性**：在查询时，需要通过关联实体进行多表连接，可能会使查询语句稍微复杂一些。

## 2. 数据库与核心模型

我们将用新的 `VariableMqttAlias` 实体来取代之前简单的多对多映射表。

### 2.1. `DbVariableMqttAlias` 实体 (`DMS.Infrastructure`)

```csharp
// 文件: DMS.Infrastructure/Entities/DbVariableMqttAlias.cs
using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库实体：对应数据库中的 VariableMqttAliases 表，用于存储变量与MQTT服务器的关联别名。
/// </summary>
[SugarTable("VariableMqttAliases")]
public class DbVariableMqttAlias
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 外键，指向 Variables 表的 Id。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 外键，指向 MqttServers 表的 Id。
    /// </summary>
    public int MqttServerId { get; set; }

    /// <summary>
    /// 针对此特定[变量-服务器]连接的发布别名。此别名将用于构建MQTT Topic。
    /// </summary>
    [SugarColumn(Length = 200)]
    public string Alias { get; set; }
}
```

### 2.2. 领域模型重构 (`DMS.Core`)

`Variable` 和 `MqttServer` 不再直接相互引用，而是都通过 `VariableMqttAlias` 集合进行关联。这意味着在领域模型层面，它们之间是“通过” `VariableMqttAlias` 建立联系的。

```csharp
// 文件: DMS.Core/Models/VariableMqttAlias.cs (新增)
namespace DMS.Core.Models;

/// <summary>
/// 领域模型：代表一个变量到一个MQTT服务器的特定关联，包含专属别名。
/// 这是一个关联实体，用于解决多对多关系中需要额外属性（别名）的问题。
/// </summary>
public class VariableMqttAlias
{
    public int Id { get; set; }
    public int VariableId { get; set; }
    public int MqttServerId { get; set; }
    public string Alias { get; set; }

    // 导航属性，方便在代码中访问关联的领域对象
    public Variable Variable { get; set; }
    public MqttServer MqttServer { get; set; }
}

// 文件: DMS.Core/Models/Variable.cs (修改)
public class Variable
{
    // ... 其他属性
    // 移除旧的直接关联：public List<MqttServer> MqttServers { get; set; }

    /// <summary>
    /// 此变量的所有MQTT发布别名关联。一个变量可以关联多个MQTT服务器，每个关联可以有独立的别名。
    /// </summary>
    public List<VariableMqttAlias> MqttAliases { get; set; } = new();
}

// 文件: DMS.Core/Models/MqttServer.cs (修改)
public class MqttServer
{
    // ... 其他属性
    // 移除旧的直接关联：public List<Variable> Variables { get; set; }

    /// <summary>
    /// 与此服务器关联的所有变量别名。通过此集合可以反向查找关联的变量。
    /// </summary>
    public List<VariableMqttAlias> VariableAliases { get; set; } = new();
}
```

## 3. 数据处理链更新 (`DMS.Infrastructure`)

### 3.1. 设计思路与考量

*   **使用别名构建Topic**：`MqttPublishProcessor` 现在必须遍历 `Variable` 的 `MqttAliases` 集合，以获取每个目标服务器及其对应的专属别名来构建MQTT Topic。这确保了发布的消息Topic符合每个MQTT Broker的特定要求。
*   **数据加载**：在处理之前，需要确保 `Variable` 对象的 `MqttAliases` 集合及其内部的 `MqttServer` 导航属性已被正确加载（通常通过仓储的 `Include` 或 `Mapper` 方法）。

### 3.2. 示例：`MqttPublishProcessor.cs`

```csharp
// 文件: DMS.Infrastructure/Services/Processors/MqttPublishProcessor.cs (修改)
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Infrastructure.Services.Communication;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using NLog;

namespace DMS.Infrastructure.Services.Processing;

/// <summary>
/// MQTT发布处理器，负责将变量值发布到关联的MQTT服务器，并使用专属别名。
/// </summary>
public class MqttPublishProcessor : VariableProcessorBase
{
    private readonly IMqttPublishService _mqttService;
    private readonly IRepositoryManager _repoManager; // 使用 RepositoryManager 来获取仓储
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MqttPublishProcessor(IMqttPublishService mqttService, IRepositoryManager repoManager)
    {
        _mqttService = mqttService;
        _repoManager = repoManager;
    }

    protected override async Task HandleAsync(VariableContext context)
    {
        if (!context.IsValueChanged) return; // 如果值未变化，则不发布

        // 1. 从仓储获取变量及其完整的别名关联列表
        //    这要求 IVariableRepository 有一个方法能加载 VariableMqttAlias 及其 MqttServer
        var variableWithAliases = await _repoManager.Variables.GetVariableWithMqttAliasesAsync(context.Variable.Id);
        
        if (variableWithAliases?.MqttAliases == null || !variableWithAliases.MqttAliases.Any())
        {
            return; // 没有关联的MQTT服务器，无需发布
        }

        foreach (var aliasInfo in variableWithAliases.MqttAliases)
        {
            try
            {
                // 确保 MqttServer 导航属性已加载且激活
                var targetServer = aliasInfo.MqttServer;
                if (targetServer == null || !targetServer.IsActive)
                {
                    _logger.Warn($"MQTT发布失败：变量 {context.Variable.Name} 关联的MQTT服务器 {aliasInfo.MqttServerId} 不存在或未激活。");
                    continue;
                }

                // 使用别名构建Topic
                // 示例Topic格式：DMS/DeviceName/VariableAlias
                var topic = $"DMS/{context.Variable.VariableTable.Device.Name}/{aliasInfo.Alias}";
                var payload = JsonSerializer.Serialize(new { value = context.CurrentValue, timestamp = context.Timestamp });

                await _mqttService.PublishAsync(targetServer, topic, payload);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"MQTT发布失败：变量 {context.Variable.Name} 到服务器 {aliasInfo.MqttServer.ServerName}，别名 {aliasInfo.Alias}");
            }
        }
    }
}
```

## 4. 应用层支持 (`DMS.Application`)

### 4.1. 设计思路与考量

*   **UI管理**：为了让用户能够在UI上管理这些别名，应用层需要提供相应的CRUD（创建、读取、更新、删除）服务。
*   **DTOs**：定义 `VariableMqttAliasDto` 用于在应用层和表现层之间传输别名数据。

### 4.2. 示例：`VariableMqttAliasDto.cs`

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

### 4.3. 应用服务接口扩展

```csharp
// 文件: DMS.Application/Interfaces/IMqttAliasAppService.cs
using DMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义了MQTT别名管理相关的应用服务操作。
/// </summary>
public interface IMqttAliasAppService
{
    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联。
    /// </summary>
    Task<List<VariableMqttAliasDto>> GetAliasesForVariableAsync(int variableId);

    /// <summary>
    /// 异步为变量分配或更新一个MQTT别名。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <param name="mqttServerId">MQTT服务器ID。</param>
    /// <param name="alias">要设置的别名。</param>
    Task AssignAliasAsync(int variableId, int mqttServerId, string alias);

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    /// <param name="aliasId">别名关联的ID。</param>
    /// <param name="newAlias">新的别名字符串。</param>
    Task UpdateAliasAsync(int aliasId, string newAlias);

    /// <summary>
    /// 异步移除一个MQTT别名关联。
    /// </summary>
    /// <param name="aliasId">要移除的别名关联的ID。</param>
    Task RemoveAliasAsync(int aliasId);
}
```

### 4.4. 应用服务实现

```csharp
// 文件: DMS.Application/Services/MqttAliasAppService.cs
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Application.Services;

/// <summary>
/// IMqttAliasAppService 的实现，负责管理变量与MQTT服务器的别名关联。
/// </summary>
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MqttAliasAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联。
    /// </summary>
    public async Task<List<VariableMqttAliasDto>> GetAliasesForVariableAsync(int variableId)
    {
        // 从仓储获取别名，并确保加载了关联的MqttServer信息
        var aliases = await _repoManager.VariableMqttAliases.GetAliasesWithServerInfoAsync(variableId);
        return _mapper.Map<List<VariableMqttAliasDto>>(aliases);
    }

    /// <summary>
    /// 异步为变量分配或更新一个MQTT别名。
    /// </summary>
    public async Task AssignAliasAsync(int variableId, int mqttServerId, string alias)
    {
        try
        {
            _repoManager.BeginTransaction();

            // 检查是否已存在该变量与该服务器的关联
            var existingAlias = await _repoManager.VariableMqttAliases.GetByVariableAndServerAsync(variableId, mqttServerId);

            if (existingAlias != null)
            {
                // 如果存在，则更新别名
                existingAlias.Alias = alias;
                await _repoManager.VariableMqttAliases.UpdateAsync(existingAlias);
            }
            else
            {
                // 如果不存在，则创建新的关联
                var newAlias = new VariableMqttAlias
                {
                    VariableId = variableId,
                    MqttServerId = mqttServerId,
                    Alias = alias
                };
                await _repoManager.VariableMqttAliases.AddAsync(newAlias);
            }

            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("分配/更新MQTT别名失败。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    public async Task UpdateAliasAsync(int aliasId, string newAlias)
    {
        try
        {
            _repoManager.BeginTransaction();
            var aliasToUpdate = await _repoManager.VariableMqttAliases.GetByIdAsync(aliasId);
            if (aliasToUpdate == null)
            {
                throw new KeyNotFoundException($"未找到ID为 {aliasId} 的MQTT别名关联。");
            }
            aliasToUpdate.Alias = newAlias;
            await _repoManager.VariableMqttAliases.UpdateAsync(aliasToUpdate);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("更新MQTT别名失败。", ex);
        }
    }

    /// <summary>
    /// 异步移除一个MQTT别名关联。
    /// </summary>
    public async Task RemoveAliasAsync(int aliasId)
    {
        try
        {
            _repoManager.BeginTransaction();
            await _repoManager.VariableMqttAliases.DeleteAsync(aliasId);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("移除MQTT别名失败。", ex);
        }
    }
}
```