using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DMS.Application.Services.Triggers;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models.Triggers;
using Microsoft.Extensions.Logging; // 使用标准日志接口

namespace DMS.Application.Services.Triggers.Impl
{
    /// <summary>
    /// 触发器动作执行器实现
    /// </summary>
    public class TriggerActionExecutor : ITriggerActionExecutor
    {
        // 假设这些服务将在未来实现或通过依赖注入提供
        // 目前我们将它们设为 null，并在使用时进行空检查
        private readonly IEmailService _emailService; // 假设已在项目中存在或将来实现
        private readonly ILogger<TriggerActionExecutor> _logger; // 使用标准日志接口

        public TriggerActionExecutor(
            // 可以选择性地注入这些服务，如果暂时不需要可以留空或使用占位符
            IEmailService emailService,
            ILogger<TriggerActionExecutor> logger)
        {
            _emailService = emailService; // 可能为 null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 执行触发动作
        /// </summary>
        public async Task ExecuteActionAsync(TriggerContext context)
        {
            try
            {
                switch (context.Trigger.Action)
                {
                    case ActionType.SendEmail:
                        // await ExecuteSendEmail(context);
                        break;
                    case ActionType.ActivateAlarm:
                        _logger.LogWarning("Action 'ActivateAlarm' is not implemented yet.");
                        // await ExecuteActivateAlarm(context);
                        break;
                    case ActionType.WriteToLog:
                        _logger.LogWarning("Action 'WriteToLog' is not implemented yet.");
                        // await ExecuteWriteToLog(context);
                        break;
                    default:
                        _logger.LogWarning("Unsupported action type: {ActionType}", context.Trigger.Action);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action '{ActionType}' for trigger '{TriggerId}'.", context.Trigger.Action, context.Trigger.Id);
                // 可以选择是否重新抛出异常或静默处理
                // throw; 
            }
        }

    }
}