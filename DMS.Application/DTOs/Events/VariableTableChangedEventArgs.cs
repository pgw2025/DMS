using System;

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
        /// 变量表ID
        /// </summary>
        public int VariableTableId { get; }

        /// <summary>
        /// 变量表名称
        /// </summary>
        public string VariableTableName { get; }

        /// <summary>
        /// 关联的设备ID
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="variableTableId">变量表ID</param>
        /// <param name="variableTableName">变量表名称</param>
        /// <param name="deviceId">关联的设备ID</param>
        public VariableTableChangedEventArgs(DataChangeType changeType, int variableTableId, string variableTableName, int deviceId)
        {
            ChangeType = changeType;
            VariableTableId = variableTableId;
            VariableTableName = variableTableName;
            DeviceId = deviceId;
            ChangeTime = DateTime.Now;
        }
    }
}