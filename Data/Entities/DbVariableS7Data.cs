using SqlSugar;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的S7变量数据实体，继承自DbVariableData。
/// </summary>
[SugarTable("S7DataVariable")]
public class DbVariableS7Data : DbVariableData
{
}