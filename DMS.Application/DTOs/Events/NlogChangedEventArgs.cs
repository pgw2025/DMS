using System;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// Nlog日志变更事件参数
    /// </summary>
    public class NlogChangedEventArgs : DataChangedEventArgs
    {
        /// <summary>
        /// 变更的日志DTO
        /// </summary>
        public NlogDto Nlog { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="nlog">变更的日志DTO</param>
        public NlogChangedEventArgs(DataChangeType changeType, NlogDto nlog) : base(changeType)
        {
            Nlog = nlog;
        }
    }
}