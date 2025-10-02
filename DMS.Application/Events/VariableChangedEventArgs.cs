using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Events
{
    /// <summary>
    /// 变量变更事件参数
    /// </summary>
    public class VariableChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public ActionChangeType ChangeType { get; }

        /// <summary>
        /// 变量DTO
        /// </summary>
        public VariableDto Variable { get; }

        /// <summary>
        /// 发生变化的属性类型
        /// </summary>
        public VariablePropertyType PropertyType { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="variable">变量DTO</param>
        /// <param name="propertyType">发生变化的属性类型</param>
        public VariableChangedEventArgs(ActionChangeType changeType, VariableDto variable, VariablePropertyType propertyType = VariablePropertyType.All)
        {
            ChangeType = changeType;
            Variable = variable;
            PropertyType = propertyType;
        }
    }
}