using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.Application.DTOs;
using DMS.Infrastructure.Services;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// 定义S7服务管理器的接口
    /// </summary>
    public interface IS7ServiceManager : IDisposable
    {
        /// <summary>
        /// 初始化服务管理器
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 添加设备到监控列表
        /// </summary>
        void AddDevice(DeviceDto device);

        /// <summary>
        /// 移除设备监控
        /// </summary>
        Task RemoveDeviceAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新设备变量
        /// </summary>
        void UpdateVariables(int deviceId, List<VariableDto> variables);

        /// <summary>
        /// 获取设备连接状态
        /// </summary>
        bool IsDeviceConnected(int deviceId);

        /// <summary>
        /// 重新连接设备
        /// </summary>
        Task ReconnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有监控的设备ID
        /// </summary>
        IEnumerable<int> GetMonitoredDeviceIds();

        /// <summary>
        /// 连接指定设备
        /// </summary>
        Task ConnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 断开指定设备连接
        /// </summary>
        Task DisconnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量连接设备
        /// </summary>
        Task ConnectDevicesAsync(IEnumerable<int> deviceIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量断开设备连接
        /// </summary>
        Task DisconnectDevicesAsync(IEnumerable<int> deviceIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有监控的设备ID
        /// </summary>
        List<S7DeviceContext> GetAllDeviceContexts();
    }
}