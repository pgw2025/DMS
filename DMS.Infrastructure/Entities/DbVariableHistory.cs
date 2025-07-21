using SqlSugar;

namespace DMS.Infrastructure.Entities;

[SugarTable("VariableHistories")]
public class DbVariableHistory
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public int VariableId { get; set; }
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
}