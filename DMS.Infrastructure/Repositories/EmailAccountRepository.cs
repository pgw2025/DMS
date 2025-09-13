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
    /// 邮件账户仓储实现
    /// </summary>
    public class EmailAccountRepository : IEmailAccountRepository
    {
        private readonly SqlSugarDbContext _dbContext;
        protected readonly ILogger<EmailAccountRepository> _logger;
        private readonly IMapper _mapper;

        public EmailAccountRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<EmailAccountRepository> logger)
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
        /// 获取默认邮件账户
        /// </summary>
        public async Task<EmailAccount> GetDefaultAccountAsync()
        {
            var dbEntity = await Db.Queryable<DbEmailAccount>()
                .Where(e => e.IsDefault && e.IsActive)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailAccount>(dbEntity) : null;
        }

        /// <summary>
        /// 获取所有启用的邮件账户
        /// </summary>
        public async Task<List<EmailAccount>> GetActiveAccountsAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailAccount>()
                .Where(e => e.IsActive)
                .ToListAsync();
            
            return _mapper.Map<List<EmailAccount>>(dbEntities);
        }

        /// <summary>
        /// 异步根据ID获取单个实体。
        /// </summary>
        public async Task<EmailAccount> GetByIdAsync(int id)
        {
            var dbEntity = await Db.Queryable<DbEmailAccount>()
                .In(id)
                .FirstAsync();
            
            return dbEntity != null ? _mapper.Map<EmailAccount>(dbEntity) : null;
        }

        /// <summary>
        /// 异步获取所有实体。
        /// </summary>
        public async Task<List<EmailAccount>> GetAllAsync()
        {
            var dbEntities = await Db.Queryable<DbEmailAccount>()
                .ToListAsync();
            
            return _mapper.Map<List<EmailAccount>>(dbEntities);
        }

        /// <summary>
        /// 异步添加一个新实体。
        /// </summary>
        public async Task<EmailAccount> AddAsync(EmailAccount entity)
        {
            var dbEntity = _mapper.Map<DbEmailAccount>(entity);
            var result = await Db.Insertable(dbEntity)
                .ExecuteReturnEntityAsync();
            
            return _mapper.Map<EmailAccount>(result);
        }

        /// <summary>
        /// 异步更新一个已存在的实体。
        /// </summary>
        public async Task<int> UpdateAsync(EmailAccount entity)
        {
            var dbEntity = _mapper.Map<DbEmailAccount>(entity);
            return await Db.Updateable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteAsync(EmailAccount entity)
        {
            var dbEntity = _mapper.Map<DbEmailAccount>(entity);
            return await Db.Deleteable(dbEntity)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID删除一个实体。
        /// </summary>
        public async Task<int> DeleteByIdAsync(int id)
        {
            return await Db.Deleteable<DbEmailAccount>()
                .In(id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步根据ID列表批量删除实体。
        /// </summary>
        public async Task<int> DeleteByIdsAsync(List<int> ids)
        {
            return await Db.Deleteable<DbEmailAccount>()
                .In(ids)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 从数据库获取数据。
        /// </summary>
        public async Task<List<EmailAccount>> TakeAsync(int number)
        {
            var dbEntities = await Db.Queryable<DbEmailAccount>()
                .Take(number)
                .ToListAsync();
            
            return _mapper.Map<List<EmailAccount>>(dbEntities);
        }

        /// <summary>
        /// 异步批量添加实体。
        /// </summary>
        public async Task<bool> AddBatchAsync(List<EmailAccount> entities)
        {
            var dbEntities = _mapper.Map<List<DbEmailAccount>>(entities);
            var result = await Db.Insertable(dbEntities)
                .ExecuteCommandAsync();
            
            return result > 0;
        }
    }
}