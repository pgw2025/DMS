using System.Threading.Tasks;

namespace DMS.Application.Services.Triggers
{
    /// <summary>
    /// 触发器动作执行器接口 (负责执行具体的触发动作)
    /// </summary>
    public interface ITriggerActionExecutor
    {
        /// <summary>
        /// 执行触发动作
        /// </summary>
        /// <param name="context">触发上下文</param>
        /// <returns>任务</returns>
        Task ExecuteActionAsync(TriggerContext context);
    }
}