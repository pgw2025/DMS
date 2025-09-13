using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories
{
    /// <summary>
    /// 报警历史记录仓库实现
    /// </summary>
    public class AlarmHistoryRepository : BaseRepository<AlarmHistory>, IAlarmHistoryRepository
    {
        public AlarmHistoryRepository(SqlSugarDbContext dbContext,ILogger<AlarmHistoryRepository> logger) : base(dbContext,logger)
        {
        }
        
        // 可以添加特定于报警历史记录的查询方法的实现
        // 例如：
        // public async Task<IEnumerable<AlarmHistory>> GetUnacknowledgedAlarmsAsync()
        // {
        //     return await _db.Queryable<AlarmHistory>()
        //         .Where(a => !a.IsAcknowledged)
        //         .ToListAsync();
        // }
    }
}