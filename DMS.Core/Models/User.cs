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