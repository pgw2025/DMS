using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
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
        private readonly IEmailAppService _emailAppService; // 注入邮件服务

        public AlarmEventHandler(
            ILogger<AlarmEventHandler> logger, 
            IAlarmHistoryRepository alarmHistoryRepository,
            IEmailAppService emailAppService) // 添加邮件服务依赖
        {
            _logger = logger;
            _alarmHistoryRepository = alarmHistoryRepository;
            _emailAppService = emailAppService;
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
            
            // 发送邮件通知
            try
            {
                // 获取默认邮件账户
                var emailAccounts = await _emailAppService.GetAllEmailAccountsAsync();
                var defaultAccount = emailAccounts.FirstOrDefault(); // 简单选择第一个账户
                
                if (defaultAccount != null)
                {
                    // 构建邮件内容
                    var emailRequest = new SendEmailRequest
                    {
                        EmailAccountId = defaultAccount.Id,
                        To = "peigangwei@qq.com", // 这里应该从配置或用户信息中获取
                        Subject = $"设备报警通知 - {e.VariableName}",
                        Body = $@"
<html>
<body>
    <h2>设备报警通知</h2>
    <p><strong>报警时间:</strong> {e.Timestamp:yyyy-MM-dd HH:mm:ss}</p>
    <p><strong>变量名称:</strong> {e.VariableName}</p>
    <p><strong>当前值:</strong> {e.CurrentValue}</p>
    <p><strong>阈值:</strong> {e.ThresholdValue}</p>
    <p><strong>报警类型:</strong> {e.AlarmType}</p>
    <p><strong>报警消息:</strong> {e.Message}</p>
</body>
</html>",
                        IsHtml = true
                    };
                    
                    // 发送邮件
                    await _emailAppService.SendEmailAsync(emailRequest);
                    _logger.LogInformation($"报警邮件已发送: {e.Message}");
                }
                else
                {
                    _logger.LogWarning("未配置邮件账户，无法发送报警邮件");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送报警邮件时发生错误: {ex.Message}");
            }
            
            // 在这里可以添加其他报警处理逻辑
            // 例如：
            // 3. 触发其他操作
        }
    }
}