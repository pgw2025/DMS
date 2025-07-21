using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库实体：对应数据库中的 VariableMqttAliases 表。
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
    /// 针对此特定[变量-服务器]连接的发布别名。
    /// </summary>
    public string Alias { get; set; }
}