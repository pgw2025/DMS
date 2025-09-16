using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.Application.DTOs;
// 明确指定 Timer 类型，避免歧义
using ThreadingTimer = System.Threading.Timer;
using TimersTimer = System.Timers.Timer;
using DMS.Application.Services.Triggers;
using DMS.Core.Models.Triggers;
using Microsoft.Extensions.Logging; // 使用 Microsoft.Extensions.Logging.ILogger

namespace DMS.Application.Services.Triggers.Impl
{
    /// <summary>
    /// 触发器评估服务实现
    /// </summary>
    public class TriggerEvaluationService : ITriggerEvaluationService, IDisposable
    {
        private readonly ITriggerManagementService _triggerManagementService;
        // 移除了 IVariableAppService 依赖
        private readonly ITriggerActionExecutor _actionExecutor;
        private readonly ILogger<TriggerEvaluationService> _logger; // 使用标准日志接口
        // 为每个触发器存储抑制定时器
        private readonly ConcurrentDictionary<int, ThreadingTimer> _suppressionTimers = new();

        public TriggerEvaluationService(
            ITriggerManagementService triggerManagementService,
            // IVariableAppService variableAppService, // 移除此参数
            ITriggerActionExecutor actionExecutor,
            ILogger<TriggerEvaluationService> logger) // 使用标准日志接口
        {
            _triggerManagementService = triggerManagementService ?? throw new ArgumentNullException(nameof(triggerManagementService));
            // _variableAppService = variableAppService ?? throw new ArgumentNullException(nameof(variableAppService));
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 评估与指定变量关联的所有激活状态的触发器
        /// </summary>
        public async Task EvaluateTriggersAsync(int variableId, object currentValue)
        {
            try
            {
                var triggers = await _triggerManagementService.GetTriggersForVariableAsync(variableId);
                // 注意：这里不再通过 _variableAppService 获取 VariableDto，
                // 而是在调用 ExecuteActionAsync 时，由上层（DataEventService）提供。
                // 如果需要 VariableDto 信息，可以在 ExecuteActionAsync 的 TriggerContext 中携带。

                _logger.LogDebug($"Evaluating {triggers.Count(t => t.IsActive)} active triggers for variable ID: {variableId}");

                foreach (var trigger in triggers.Where(t => t.IsActive))
                {
                    if (!IsWithinSuppressionWindow(trigger)) // Check suppression first
                    {
                        if (EvaluateCondition(trigger, currentValue))
                        {
                            // 创建一个临时的上下文对象，其中 VariableDto 可以为 null，
                            // 因为我们目前没有从 _variableAppService 获取它。
                            // 在实际应用中，你可能需要通过某种方式获取 VariableDto。
                            var context = new TriggerContext(trigger, currentValue, null); 

                            await _actionExecutor.ExecuteActionAsync(context);

                            // Update last triggered time and start suppression timer if needed
                            trigger.LastTriggeredAt = DateTime.UtcNow;
                            // For simplicity, we'll assume it's updated periodically or on next load.
                            // In a production scenario, you'd likely want to persist this back to the database.

                             // Start suppression timer if duration is set (in-memory suppression)
                             if (trigger.SuppressionDuration.HasValue)
                             {
                                // 使用 ThreadingTimer 避免歧义
                                var timer = new ThreadingTimer(_ =>
                                {
                                    trigger.LastTriggeredAt = null; // Reset suppression flag after delay
                                    _logger.LogInformation($"Suppression lifted for trigger {trigger.Id}");
                                    // Note: Modifying 'trigger' directly affects the object in the list returned by GetTriggersForVariableAsync().
                                    // This works for in-memory state but won't persist changes. Consider updating DB explicitly if needed.
                                }, null, trigger.SuppressionDuration.Value, Timeout.InfiniteTimeSpan); // Single shot timer
                                
                                // Replace any existing timer for this trigger ID
                                _suppressionTimers.AddOrUpdate(trigger.Id, timer, (key, oldTimer) => {
                                    oldTimer?.Dispose();
                                    return timer;
                                });
                             }
                        }
                     }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while evaluating triggers for variable ID: {VariableId}", variableId);
            }
        }

        /// <summary>
        /// 内部方法：评估单个触发器的条件
        /// </summary>
        private bool EvaluateCondition(TriggerDefinitionDto trigger, object currentValueObj)
        {
            if (currentValueObj == null) 
            {
                _logger.LogWarning("Cannot evaluate trigger condition: Current value is null for trigger ID: {TriggerId}", trigger.Id);
                return false; // Cannot evaluate null
            }
            
            // Attempt conversion from object to double - adjust parsing logic as needed for your data types
            if (!double.TryParse(currentValueObj.ToString(), out double currentValue))
            {
                _logger.LogWarning("Could not parse current value '{CurrentValue}' to double for trigger evaluation (trigger ID: {TriggerId}).", currentValueObj, trigger.Id);
                return false;
            }

            bool result = trigger.Condition switch
            {
                ConditionType.GreaterThan => currentValue > trigger.Threshold,
                ConditionType.LessThan => currentValue < trigger.Threshold,
                ConditionType.EqualTo => Math.Abs(currentValue - trigger.Threshold.GetValueOrDefault()) < double.Epsilon,
                ConditionType.NotEqualTo => Math.Abs(currentValue - trigger.Threshold.GetValueOrDefault()) >= double.Epsilon,
                ConditionType.InRange => currentValue >= trigger.LowerBound && currentValue <= trigger.UpperBound,
                ConditionType.OutOfRange => currentValue < trigger.LowerBound || currentValue > trigger.UpperBound,
                _ => false
            };

            if(result)
            {
                 _logger.LogInformation("Trigger condition met: Variable value {CurrentValue} satisfies {Condition} for trigger ID: {TriggerId}", 
                     currentValue, trigger.Condition, trigger.Id);
            }

            return result;
        }

        /// <summary>
        /// 内部方法：检查触发器是否处于抑制窗口期内
        /// </summary>
        private bool IsWithinSuppressionWindow(TriggerDefinitionDto trigger)
        {
            if (!trigger.SuppressionDuration.HasValue || !trigger.LastTriggeredAt.HasValue)
                return false;

            var suppressionEndTime = trigger.LastTriggeredAt.Value.Add(trigger.SuppressionDuration.Value);
            bool isSuppressed = DateTime.UtcNow < suppressionEndTime;
            
            if(isSuppressed)
            {
                _logger.LogTrace("Trigger is suppressed (until {SuppressionEnd}) for trigger ID: {TriggerId}", suppressionEndTime, trigger.Id);
            }
            
            return isSuppressed;
        }

        /// <summary>
        /// 实现 IDisposable 以清理计时器资源
        /// </summary>
        public void Dispose()
        {
            foreach (var kvp in _suppressionTimers)
            {
                kvp.Value?.Dispose();
            }
            _suppressionTimers.Clear();
        }
    }
}