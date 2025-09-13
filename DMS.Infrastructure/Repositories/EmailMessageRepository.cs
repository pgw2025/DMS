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
    /// 邮件消息仓储实现
    /// </summary>
    public class EmailMessageRepository : IEmailMessageRepository
    {
        private readonly SqlSugarDbContext _dbContext;
        protected readonly ILogger<EmailMessageRepository> _logger;
        private readonly IMapper _mapper;

        public EmailMessageRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<EmailMessageRepository> logger)
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
        public async Task<EmailMessage> GetByIdAsync(int id)
        {
            var dbEntity = await Db.Queryable<DbEmailMessage>()
                .In(id)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailMessage>(dbEntity) : null;
        }

        /// <summary>
        /// 异步获取所有实体。
        /// </summary>
        public async Task<List<EmailMessage>> GetAllAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailMessage>()
                .ToListAsync();
            
            return _mapper.Map<List<EmailMessage>>(dbEntities);
        }

        /// <summary>
        /// 异步添加一个新实体。
        /// </summary>
        public async Task<EmailMessage> AddAsync(EmailMessage entity)
        {
            var dbEntity = _mapper.Map<DbEmailMessage>(entity);
            var result = await Db.Insertable(dbEntity)
                .ExecuteReturnEntityAsync();
            
            return _mapper.Map<EmailMessage>(result);
        }

        /// <summary>
        /// 异步更新一个已存在的实体。
        /// </summary>
        public async Task<int> UpdateAsync(EmailMessage entity)
        {
            var dbEntity = _mapper.Map<DbEmailMessage>(entity);
            return await Db.Updateable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteAsync(EmailMessage entity)
        {
            var dbEntity = _mapper.Map<DbEmailMessage>(entity);
            return await Db.Deleteable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteByIdAsync(int id)
        {
            return await Db.Deleteable<DbEmailMessage>()
                .In(id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID列表批量删除实体。
        /// </summary>
        public async Task<int> DeleteByIdsAsync(List<int> ids)
        {
            return await Db.Deleteable<DbEmailMessage>()
                .In(ids)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 从数据库获取数据。
        /// </summary>
        public async Task<List<EmailMessage>> TakeAsync(int number)
        {
            var dbEntities = await Db.Queryable<DbEmailMessage>()
                .Take(number)
                .ToListAsync();
            
            return _mapper.Map<List<EmailMessage>>(dbEntities);
        }

        /// <summary>
        /// 异步批量添加实体。
        /// </summary>
        public async Task<bool> AddBatchAsync(List<EmailMessage> entities)
        {
            var dbEntities = _mapper.Map<List<DbEmailMessage>>(entities);
            var result = await Db.Insertable(dbEntities)
                .ExecuteCommandAsync();
            
            return result > 0;
        }

        /// <summary>
        /// 根据状态获取邮件消息
        /// </summary>
        public async Task<List<EmailMessage>> GetByStatusAsync(EmailSendStatus status)
        {
            var dbEntities = await Db.Queryable<DbEmailMessage>()
                .Where(e => e.Status == status.ToString())
                .ToListAsync();
            
            return _mapper.Map<List<EmailMessage>>(dbEntities);
        }

        /// <summary>
        /// 获取指定时间范围内的邮件消息
        /// </summary>
        public async Task<List<EmailMessage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var dbEntities = await Db.Queryable<DbEmailMessage>()
                .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .ToListAsync();
            
            return _mapper.Map<List<EmailMessage>>(dbEntities);
        }
    }
}