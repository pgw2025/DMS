using SqlSugar;

namespace DMS.Infrastructure.Entities;

public class DbVariableTable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int DeviceId { get; set; }
    public int Protocol { get; set; } // 对应 ProtocolType 枚举
}