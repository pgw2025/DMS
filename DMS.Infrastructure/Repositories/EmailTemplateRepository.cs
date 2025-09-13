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
    /// 邮件模板仓储实现
    /// </summary>
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly SqlSugarDbContext _dbContext;
        protected readonly ILogger<EmailTemplateRepository> _logger;
        private readonly IMapper _mapper;

        public EmailTemplateRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<EmailTemplateRepository> logger)
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
        public async Task<EmailTemplate> GetByIdAsync(int id)
        {
            var dbEntity = await Db.Queryable<DbEmailTemplate>()
                .In(id)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailTemplate>(dbEntity) : null;
        }

        /// <summary>
        /// 异步获取所有实体。
        /// </summary>
        public async Task<List<EmailTemplate>> GetAllAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailTemplate>()
                .ToListAsync();
            
            return _mapper.Map<List<EmailTemplate>>(dbEntities);
        }

        /// <summary>
        /// 异步添加一个新实体。
        /// </summary>
        public async Task<EmailTemplate> AddAsync(EmailTemplate entity)
        {
            var dbEntity = _mapper.Map<DbEmailTemplate>(entity);
            var result = await Db.Insertable(dbEntity)
                .ExecuteReturnEntityAsync();
            
            return _mapper.Map<EmailTemplate>(result);
        }

        /// <summary>
        /// 异步更新一个已存在的实体。
        /// </summary>
        public async Task<int> UpdateAsync(EmailTemplate entity)
        {
            var dbEntity = _mapper.Map<DbEmailTemplate>(entity);
            return await Db.Updateable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteAsync(EmailTemplate entity)
        {
            var dbEntity = _mapper.Map<DbEmailTemplate>(entity);
            return await Db.Deleteable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteByIdAsync(int id)
        {
            return await Db.Deleteable<DbEmailTemplate>()
                .In(id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID列表批量删除实体。
        /// </summary>
        public async Task<int> DeleteByIdsAsync(List<int> ids)
        {
            return await Db.Deleteable<DbEmailTemplate>()
                .In(ids)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 从数据库获取数据。
        /// </summary>
        public async Task<List<EmailTemplate>> TakeAsync(int number)
        {
            var dbEntities = await Db.Queryable<DbEmailTemplate>()
                .Take(number)
                .ToListAsync();
            
            return _mapper.Map<List<EmailTemplate>>(dbEntities);
        }

        /// <summary>
        /// 异步批量添加实体。
        /// </summary>
        public async Task<bool> AddBatchAsync(List<EmailTemplate> entities)
        {
            var dbEntities = _mapper.Map<List<DbEmailTemplate>>(entities);
            var result = await Db.Insertable(dbEntities)
                .ExecuteCommandAsync();
            
            return result > 0;
        }

        /// <summary>
        /// 根据代码获取邮件模板
        /// </summary>
        public async Task<EmailTemplate> GetByCodeAsync(string code)
        {
            var dbEntity = await Db.Queryable<DbEmailTemplate>()
                .Where(e => e.Code == code && e.IsActive)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailTemplate>(dbEntity) : null;
        }

        /// <summary>
        /// 获取所有启用的邮件模板
        /// </summary>
        public async Task<List<EmailTemplate>> GetActiveTemplatesAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailTemplate>()
                .Where(e => e.IsActive)
                .ToListAsync();
            
            return _mapper.Map<List<EmailTemplate>>(dbEntities);
        }
    }
}