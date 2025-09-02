using System;

namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 数据变更事件参数基类
    /// </summary>
    public class DataChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        public DataChangedEventArgs(DataChangeType changeType)
        {
            ChangeType = changeType;
            ChangeTime = DateTime.Now;
        }
    }
}