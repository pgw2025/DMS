using DMS.Application.Interfaces;
using DMS.Application.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors
{
    public class AlarmProcessor : IVariableProcessor
    {
        private readonly IAlarmService _alarmService;
        private readonly ILogger<AlarmProcessor> _logger;

        public AlarmProcessor(IAlarmService alarmService, ILogger<AlarmProcessor> logger)
        {
            _alarmService = alarmService;
            _logger = logger;
        }

        public async Task ProcessAsync(VariableContext context)
        {
            try
            {
                // 检查是否触发报警
                bool isAlarmTriggered = _alarmService.CheckAlarm(context.Data);
                
                if (isAlarmTriggered)
                {
                    _logger.LogInformation($"变量 {context.Data.Name} 触发了报警。");
                    // 报警逻辑已经通过事件处理，这里可以添加其他处理逻辑（如记录到数据库）
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理变量 {context.Data.Name} 的报警时发生错误: {ex.Message}");
            }
            
            // 不设置 context.IsHandled = true，让其他处理器继续处理
        }
    }
}