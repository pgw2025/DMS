using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.Interfaces.Management;
using DMS.Application.Services;
using DMS.Application.Services.Management;
using DMS.Core.Models;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义数据管理相关的应用服务操作，负责管理所有的数据，包括设备、变量表和变量。
/// </summary>
public interface IAppCenterService
{
    ILogManagementService LogManagementService { get; set; }
    IMqttManagementService MqttManagementService { get; set; }
    IMenuManagementService MenuManagementService { get; set; }
    IVariableManagementService VariableManagementService { get; set; }
    IVariableTableManagementService VariableTableManagementService { get; set; }
    IDeviceManagementService DeviceManagementService { get; set; }
    ITriggerManagementService TriggerManagementService { get; set; } // 添加触发器管理服务
    IDataLoaderService DataLoaderService { get; set; }
}