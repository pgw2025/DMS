using System;
using DMS.Core.Enums;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 变量表变更事件参数
    /// </summary>
    public class VariableTableChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// 变量表DTO
        /// </summary>
        public VariableTableDto VariableTable { get; }

        /// <summary>
        /// 关联的设备DTO
        /// </summary>
        public DeviceDto Device { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="variableTable">变量表DTO</param>
        /// <param name="device">关联的设备DTO</param>
        public VariableTableChangedEventArgs(DataChangeType changeType, VariableTableDto variableTable, DeviceDto device)
        {
            ChangeType = changeType;
            VariableTable = variableTable;
            Device = device;
            ChangeTime = DateTime.Now;
        }
    }
}