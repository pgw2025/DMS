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
        /// 加载的设备数量
        /// </summary>
        public int DeviceCount { get; }

        /// <summary>
        /// 加载的变量表数量
        /// </summary>
        public int VariableTableCount { get; }

        /// <summary>
        /// 加载的变量数量
        /// </summary>
        public int VariableCount { get; }

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
        /// <param name="deviceCount">设备数量</param>
        /// <param name="variableTableCount">变量表数量</param>
        /// <param name="variableCount">变量数量</param>
        /// <param name="isSuccess">是否成功</param>
        /// <param name="errorMessage">错误信息</param>
        public DataLoadCompletedEventArgs(int deviceCount, int variableTableCount, int variableCount, bool isSuccess, string errorMessage = null)
        {
            DeviceCount = deviceCount;
            VariableTableCount = variableTableCount;
            VariableCount = variableCount;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            LoadTime = DateTime.Now;
        }
    }
}