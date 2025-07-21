using SqlSugar;

namespace DMS.Infrastructure.Entities;

[SugarTable("Users")]
public class DbUser
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
}