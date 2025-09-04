namespace DMS.Core.Models
{
    /// <summary>
    /// 变量值变更事件参数
    /// </summary>
    public class VariableValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变量ID
        /// </summary>
        public int VariableId { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VariableValueChangedEventArgs(int variableId, string variableName, string oldValue, string newValue, DateTime updateTime)
        {
            VariableId = variableId;
            VariableName = variableName;
            OldValue = oldValue;
            NewValue = newValue;
            UpdateTime = updateTime;
        }
    }
}