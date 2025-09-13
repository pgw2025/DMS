using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// 报警历史记录仓库接口
    /// </summary>
    public interface IAlarmHistoryRepository : IBaseRepository<AlarmHistory>
    {
        // 可以添加特定于报警历史记录的查询方法
        // 例如：
        // Task<IEnumerable<AlarmHistory>> GetUnacknowledgedAlarmsAsync();
        // Task<IEnumerable<AlarmHistory>> GetAlarmsByVariableIdAsync(int variableId);
        // Task AcknowledgeAlarmAsync(int alarmId, string acknowledgedBy);
    }
}