using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

[SugarTable("Menu")]
public class DbMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public  int Id { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MenuType Type { get; set; }
    
    public int DataId { get; set; }
    [SugarColumn(IsIgnore = true)]
    public Object? Data { get; set; }
    
    [SugarColumn(IsIgnore = true)]
    public List<DbMenu> Items { get; set; }
}