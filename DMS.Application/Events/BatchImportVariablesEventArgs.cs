using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Events
{
    /// <summary>
    /// 批量导入变量事件参数
    /// </summary>
    public class BatchImportVariablesEventArgs : System.EventArgs
    {
        /// <summary>
        /// 导入的变量列表
        /// </summary>
        public List<Variable> Variables { get; }

        /// <summary>
        /// 导入的变量数量
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="variables">导入的变量列表</param>
        public BatchImportVariablesEventArgs(List<Variable> variables)
        {
            Variables = variables ?? new List<Variable>();
            Count = Variables.Count;
            ChangeTime = DateTime.Now;
        }
    }
}