using DMS.Core.Models;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于创建新设备及其关联的变量表和菜单的DTO。
/// </summary>
public class CreateDeviceWithDetailsDto
{
            public Device Device { get; set; }    public VariableTable VariableTable { get; set; }

    public MenuBean DeviceMenu { get; set; } // 如果需要包含菜单信息
    public MenuBean VariableTableMenu { get; set; } // 如果需要包含菜单信息
}