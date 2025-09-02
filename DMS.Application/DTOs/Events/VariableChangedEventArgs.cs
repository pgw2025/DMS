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
        /// 变量DTO
        /// </summary>
        public VariableDto Variable { get; }

        /// <summary>
        /// 关联的变量表DTO
        /// </summary>
        public VariableTableDto VariableTable { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="variable">变量DTO</param>
        /// <param name="variableTable">关联的变量表DTO</param>
        public VariableChangedEventArgs(DataChangeType changeType, VariableDto variable, VariableTableDto variableTable)
        {
            ChangeType = changeType;
            Variable = variable;
            VariableTable = variableTable;
            ChangeTime = DateTime.Now;
        }
    }
}