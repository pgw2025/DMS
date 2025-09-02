using System;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 变量变更事件参数
    /// </summary>
    public class VariableChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// 变量ID
        /// </summary>
        public int VariableId { get; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// 关联的变量表ID
        /// </summary>
        public int VariableTableId { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="variableId">变量ID</param>
        /// <param name="variableName">变量名称</param>
        /// <param name="variableTableId">关联的变量表ID</param>
        public VariableChangedEventArgs(DataChangeType changeType, int variableId, string variableName, int variableTableId)
        {
            ChangeType = changeType;
            VariableId = variableId;
            VariableName = variableName;
            VariableTableId = variableTableId;
            ChangeTime = DateTime.Now;
        }
    }
}