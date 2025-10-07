
using DMS.Core.Models;

namespace DMS.Application.Events
{
    /// <summary>
    /// 变量值变更事件参数
    /// </summary>
    public class VariableValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变量DTO对象
        /// </summary>
        public Variable Variable { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VariableValueChangedEventArgs(Variable variable, string? oldValue)
        {
            Variable = variable;
            OldValue = oldValue;
        }
    }
}