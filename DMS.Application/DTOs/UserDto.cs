namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示用户信息的DTO。
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
}