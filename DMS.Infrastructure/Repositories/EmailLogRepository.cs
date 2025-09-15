using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DMS.Infrastructure.Repositories
{
    /// <summary>
    /// 邮件日志仓储实现
    /// </summary>
    public class EmailLogRepository : IEmailLogRepository
    {
        private readonly SqlSugarDbContext _dbContext;
        protected readonly ILogger<EmailLogRepository> _logger;
        private readonly IMapper _mapper;

        public EmailLogRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<EmailLogRepository> logger)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取 SqlSugarClient 实例
        /// </summary>
        protected SqlSugarClient Db
        {
            get { return _dbContext.GetInstance(); }
        }

        /// <summary>
        /// 异步根据ID获取单个实体。
        /// </summary>
        public async Task<EmailLog> GetByIdAsync(int id)
        {
            var dbEntity = await Db.Queryable<DbEmailLog>()
                .In(id)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailLog>(dbEntity) : null;
        }

        /// <summary>
        /// 异步获取所有实体。
        /// </summary>
        public async Task<List<EmailLog>> GetAllAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailLog>()
                .ToListAsync();
            
            return _mapper.Map<List<EmailLog>>(dbEntities);
        }

        /// <summary>
        /// 异步添加一个新实体。
        /// </summary>
        public async Task<EmailLog> AddAsync(EmailLog entity)
        {
            var dbEntity = _mapper.Map<DbEmailLog>(entity);
            var result = await Db.Insertable(dbEntity)
                .ExecuteReturnEntityAsync();
            
            return _mapper.Map<EmailLog>(result);
        }

        /// <summary>
        /// 异步更新一个已存在的实体。
        /// </summary>
        public async Task<int> UpdateAsync(EmailLog entity)
        {
            var dbEntity = _mapper.Map<DbEmailLog>(entity);
            return await Db.Updateable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteAsync(EmailLog entity)
        {
            var dbEntity = _mapper.Map<DbEmailLog>(entity);
            return await Db.Deleteable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteByIdAsync(int id)
        {
            return await Db.Deleteable<DbEmailLog>()
                .In(id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID列表批量删除实体。
        /// </summary>
        public async Task<int> DeleteByIdsAsync(List<int> ids)
        {
            return await Db.Deleteable<DbEmailLog>()
                .In(ids)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 从数据库获取数据。
        /// </summary>
        public async Task<List<EmailLog>> TakeAsync(int number)
        {
            var dbEntities = await Db.Queryable<DbEmailLog>()
                .Take(number)
                .ToListAsync();
            
            return _mapper.Map<List<EmailLog>>(dbEntities);
        }

        /// <summary>
        /// 异步批量添加实体。
        /// </summary>
        public async Task<List<EmailLog>> AddBatchAsync(List<EmailLog> entities)
        {
            var dbEntities = _mapper.Map<List<DbEmailLog>>(entities);
            var insertedEntities = new List<DbEmailLog>();
            
            // 使用循环逐个插入实体，这样可以确保返回每个插入的实体
            foreach (var entity in dbEntities)
            {
                var insertedEntity = await Db.Insertable(entity).ExecuteReturnEntityAsync();
                insertedEntities.Add(insertedEntity);
            }
            
            return _mapper.Map<List<EmailLog>>(insertedEntities);
        }

        /// <summary>
        /// 根据邮件消息ID获取日志
        /// </summary>
        public async Task<List<EmailLog>> GetByEmailMessageIdAsync(int emailMessageId)
        {
            var dbEntities = await Db.Queryable<DbEmailLog>()
                .Where(e => e.EmailMessageId == emailMessageId)
                .ToListAsync();
            
            return _mapper.Map<List<EmailLog>>(dbEntities);
        }

        /// <summary>
        /// 根据日期范围获取日志
        /// </summary>
        public async Task<List<EmailLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var dbEntities = await Db.Queryable<DbEmailLog>()
                .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .ToListAsync();
            
            return _mapper.Map<List<EmailLog>>(dbEntities);
        }
    }
}