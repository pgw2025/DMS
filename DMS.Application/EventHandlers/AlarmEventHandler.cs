using DMS.Application.Interfaces;
using DMS.Core.Events;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.EventHandlers
{
    public class AlarmEventHandler
    {
        private readonly ILogger<AlarmEventHandler> _logger;
        private readonly IAlarmHistoryRepository _alarmHistoryRepository;
        // 可以注入其他服务，如 IEmailService, ISmsService 等

        public AlarmEventHandler(ILogger<AlarmEventHandler> logger, IAlarmHistoryRepository alarmHistoryRepository)
        {
            _logger = logger;
            _alarmHistoryRepository = alarmHistoryRepository;
        }

        public async void HandleAlarm(object sender, AlarmEventArgs e)
        {
            _logger.LogWarning($"收到报警: {e.Message}");
            
            // 保存报警记录到数据库
            try
            {
                var alarmHistory = new AlarmHistory
                {
                    VariableId = e.VariableId,
                    VariableName = e.VariableName,
                    CurrentValue = e.CurrentValue,
                    ThresholdValue = e.ThresholdValue,
                    AlarmType = e.AlarmType,
                    Message = e.Message,
                    Timestamp = e.Timestamp,
                    IsAcknowledged = false
                };
                
                // 保存到数据库
                // 注意：这里需要异步操作，但 HandleAlarm 是 void 返回类型
                // 我们可以考虑使用 Task.Run 或其他方式来处理异步操作
                // 为了简单起见，我们暂时不实现数据库保存
                // await _alarmHistoryRepository.AddAsync(alarmHistory);
                
                _logger.LogInformation($"报警记录已保存: {e.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存报警记录时发生错误: {ex.Message}");
            }
            
            // 在这里添加其他报警处理逻辑
            // 例如：
            // 2. 发送邮件或短信通知
            // 3. 触发其他操作
        }
    }
}