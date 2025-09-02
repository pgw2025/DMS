using System;
using System.Collections.Generic;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 数据加载完成事件参数
    /// </summary>
    public class DataLoadCompletedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 加载的设备列表
        /// </summary>
        public List<DeviceDto> Devices { get; }

        /// <summary>
        /// 加载的变量表列表
        /// </summary>
        public List<VariableTableDto> VariableTables { get; }

        /// <summary>
        /// 加载的变量列表
        /// </summary>
        public List<VariableDto> Variables { get; }

        /// <summary>
        /// 加载是否成功
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 加载时间
        /// </summary>
        public DateTime LoadTime { get; }

        /// <summary>
        /// 错误信息（如果加载失败）
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="devices">设备列表</param>
        /// <param name="variableTables">变量表列表</param>
        /// <param name="variables">变量列表</param>
        /// <param name="isSuccess">是否成功</param>
        /// <param name="errorMessage">错误信息</param>
        public DataLoadCompletedEventArgs(List<DeviceDto> devices, List<VariableTableDto> variableTables, List<VariableDto> variables, bool isSuccess, string errorMessage = null)
        {
            Devices = devices ?? new List<DeviceDto>();
            VariableTables = variableTables ?? new List<VariableTableDto>();
            Variables = variables ?? new List<VariableDto>();
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            LoadTime = DateTime.Now;
        }
    }
}