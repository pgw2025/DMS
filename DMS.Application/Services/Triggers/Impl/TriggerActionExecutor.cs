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
                        await ExecuteSendEmail(context);
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

        #region 私有执行方法

        private async Task ExecuteSendEmail(TriggerContext context)
        {
            if (_emailService == null)
            {
                _logger.LogWarning("Email service is not configured, skipping SendEmail action for trigger '{TriggerId}'.", context.Trigger.Id);
                return;
            }

            var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(context.Trigger.ActionConfigurationJson);
            if (config == null ||
                !config.TryGetValue("Recipients", out var recipientsElement) ||
                !config.TryGetValue("SubjectTemplate", out var subjectTemplateElement) ||
                !config.TryGetValue("BodyTemplate", out var bodyTemplateElement))
            {
                _logger.LogError("Invalid configuration for SendEmail action for trigger '{TriggerId}'.", context.Trigger.Id);
                return;
            }

            var recipients = recipientsElement.Deserialize<List<string>>();
            var subjectTemplate = subjectTemplateElement.GetString();
            var bodyTemplate = bodyTemplateElement.GetString();

            if (recipients == null || string.IsNullOrEmpty(subjectTemplate) || string.IsNullOrEmpty(bodyTemplate))
            {
                _logger.LogError("Missing required fields in SendEmail configuration for trigger '{TriggerId}'.", context.Trigger.Id);
                return;
            }

            // Simple token replacement - in practice, use a templating engine like Scriban, RazorLight etc.
            // Note: This assumes context.Variable and context.CurrentValue have Name properties/values.
            // You might need to adjust the token names and values based on your actual VariableDto structure.
            var subject = subjectTemplate
                .Replace("{VariableName}", context.Variable?.Name ?? "Unknown")
                .Replace("{CurrentValue}", context.CurrentValue?.ToString() ?? "N/A")
                .Replace("{Threshold}", context.Trigger.Threshold?.ToString() ?? "N/A")
                .Replace("{LowerBound}", context.Trigger.LowerBound?.ToString() ?? "N/A")
                .Replace("{UpperBound}", context.Trigger.UpperBound?.ToString() ?? "N/A");

            var body = bodyTemplate
                .Replace("{VariableName}", context.Variable?.Name ?? "Unknown")
                .Replace("{CurrentValue}", context.CurrentValue?.ToString() ?? "N/A")
                .Replace("{Threshold}", context.Trigger.Threshold?.ToString() ?? "N/A")
                .Replace("{LowerBound}", context.Trigger.LowerBound?.ToString() ?? "N/A")
                .Replace("{UpperBound}", context.Trigger.UpperBound?.ToString() ?? "N/A")
                .Replace("{Timestamp}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

             // await _emailService.SendEmailAsync(recipients, subject, body);
        }

         private async Task ExecuteActivateAlarm(TriggerContext context)
         {
             var alarmId = $"trigger_{context.Trigger.Id}_{context.Variable.Id}";
             var message = $"Trigger '{context.Trigger.Description}' activated for variable '{context.Variable.Name}' with value '{context.CurrentValue}'.";
             // 假设 INotificationService 有 RaiseAlarmAsync 方法
             // await _notificationService.RaiseAlarmAsync(alarmId, message); 
         }

         private async Task ExecuteWriteToLog(TriggerContext context)
         {
             var message = $"Trigger '{context.Trigger.Description}' activated for variable '{context.Variable.Name}' with value '{context.CurrentValue}'.";
             // 假设 ILoggingService 有 LogTriggerAsync 方法
             // await _loggingService.LogTriggerAsync(message); 
         }

        #endregion
    }
}