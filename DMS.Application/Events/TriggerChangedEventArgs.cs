using DMS.Core.Enums;
using DMS.Core.Models.Triggers;

namespace DMS.Application.Events
{
    /// <summary>
    /// 触发器更改事件参数
    /// </summary>
    public class TriggerChangedEventArgs : DataChangedEventArgs
    {
        /// <summary>
        /// 更改的触发器
        /// </summary>
        public Trigger Trigger { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">更改类型</param>
        /// <param name="trigger">触发器</param>
        public TriggerChangedEventArgs(DataChangeType changeType, Trigger trigger) : base(changeType)
        {
            Trigger = trigger;
        }
    }
}