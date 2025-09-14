using System;
using System.Threading.Tasks;

namespace DMS.Application.Services.Triggers
{
    /// <summary>
    /// 触发器评估服务接口 (负责判断变量值是否满足触发条件)
    /// </summary>
    public interface ITriggerEvaluationService
    {
        /// <summary>
        /// 评估与指定变量关联的所有激活状态的触发器
        /// </summary>
        /// <param name="variableId">变量 ID</param>
        /// <param name="currentValue">变量的当前值</param>
        /// <returns>任务</returns>
        Task EvaluateTriggersAsync(int variableId, object currentValue);
    }
}