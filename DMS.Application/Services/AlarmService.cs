using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly ILogger<AlarmService> _logger;

        public AlarmService(ILogger<AlarmService> logger)
        {
            _logger = logger;
        }

        public event EventHandler<AlarmEventArgs> OnAlarmTriggered;

        public bool CheckAlarm(Variable variable)
        {
            if (!variable.IsAlarmEnabled)
            {
                return false;
            }

            // 尝试将 DataValue 转换为 double
            if (!double.TryParse(variable.DataValue, out double currentValue))
            {
                // 如果是布尔值，我们也应该处理
                if (bool.TryParse(variable.DataValue, out bool boolValue))
                {
                    // 布尔值变化报警需要更复杂的逻辑，通常在 VariableItemViewModel 中处理
                    // 因为需要检测从 false 到 true 或从 true 到 false 的变化
                    // 这里我们暂时不处理
                    return false;
                }
                
                _logger.LogWarning($"无法将变量 {variable.Name} 的值 '{variable.DataValue}' 转换为数字进行报警检查。");
                return false;
            }

            bool isTriggered = false;
            string message = "";
            string alarmType = "";
            double thresholdValue = 0;

            // 检查上限报警
            if (variable.AlarmMaxValue > 0 && currentValue > variable.AlarmMaxValue)
            {
                isTriggered = true;
                message = $"变量 {variable.Name} 的值 {currentValue} 超过了上限 {variable.AlarmMaxValue}。";
                alarmType = "High";
                thresholdValue = variable.AlarmMaxValue;
            }
            // 检查下限报警
            else if (variable.AlarmMinValue > 0 && currentValue < variable.AlarmMinValue)
            {
                isTriggered = true;
                message = $"变量 {variable.Name} 的值 {currentValue} 低于了下限 {variable.AlarmMinValue}。";
                alarmType = "Low";
                thresholdValue = variable.AlarmMinValue;
            }
            // 检查死区报警
            // 注意：这里的实现假设我们有一个方法可以获取变量的上一次值
            // 在实际应用中，这可能需要在 VariableItemViewModel 或其他地方维护
            // 为了简化，我们假设有一个 PreviousValue 属性（但这在 DTO 中不存在）
            // 我们将在 VariableItemViewModel 中处理这个逻辑，并通过事件触发
            
            
            // 如果需要在 AlarmService 中处理死区报警，我们需要一种方式来获取上一次的值
            // 这可能需要修改 Variable 或通过其他方式传递上一次的值
            // 为了保持设计的清晰性，我们暂时不在这里实现死区报警
            // 死区报警可以在 VariableItemViewModel 中实现，当检测到值变化超过死区时触发一个事件

            if (isTriggered)
            {
                _logger.LogInformation(message);
                OnAlarmTriggered?.Invoke(this, new AlarmEventArgs(
                    variable.Id, variable.Name, currentValue, thresholdValue, message, alarmType));
            }

            return isTriggered;
        }
    }
}