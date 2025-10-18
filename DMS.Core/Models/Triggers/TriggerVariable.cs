using System;

namespace DMS.Core.Models.Triggers
{
    /// <summary>
    /// 触发器与变量关联领域模型
    /// </summary>
    public class TriggerVariable
    {
        /// <summary>
        /// 触发器与变量关联唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 外键，指向触发器定义的 Id
        /// </summary>
        public int TriggerDefinitionId { get; set; }

        /// <summary>
        /// 外键，指向变量的 Id
        /// </summary>
        public int VariableId { get; set; }
    }
}