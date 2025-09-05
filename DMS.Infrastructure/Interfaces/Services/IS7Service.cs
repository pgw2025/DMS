using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S7.Net;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// S7服务接口，定义了与S7 PLC进行通信所需的方法
    /// </summary>
    public interface IS7Service
    {
        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步连接到S7 PLC
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task ConnectAsync(string ipAddress, int port, short rack, short slot, S7.Net.CpuType cpuType);

        /// <summary>
        /// 异步断开与S7 PLC的连接
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task DisconnectAsync();

        /// <summary>
        /// 异步读取单个变量的值
        /// </summary>
        /// <param name="address">S7地址</param>
        /// <returns>表示异步操作的任务，包含读取到的值</returns>
        Task<object> ReadVariableAsync(string address);

        /// <summary>
        /// 异步读取多个变量的值
        /// </summary>
        /// <param name="addresses">S7地址列表</param>
        /// <returns>表示异步操作的任务，包含读取到的值字典</returns>
        Task<Dictionary<string, object>> ReadVariablesAsync(List<string> addresses);

        /// <summary>
        /// 异步写入单个变量的值
        /// </summary>
        /// <param name="address">S7地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>表示异步操作的任务</returns>
        Task WriteVariableAsync(string address, object value);

        /// <summary>
        /// 异步写入多个变量的值
        /// </summary>
        /// <param name="values">地址和值的字典</param>
        /// <returns>表示异步操作的任务</returns>
        Task WriteVariablesAsync(Dictionary<string, object> values);
    }
}