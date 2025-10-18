using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Application.Services.Triggers;
using DMS.Core.Interfaces;
using DMS.Core.Models.Triggers;

namespace DMS.Application.Services.Management
{
    /// <summary>
    /// 触发器管理服务实现
    /// </summary>
    public class TriggerManagementService : ITriggerManagementService
    {
        private readonly IAppStorageService _appStorageService;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public TriggerManagementService(IAppStorageService appStorageService,IRepositoryManager repositoryManager, IMapper mapper)
        {
            _appStorageService = appStorageService;
            _repositoryManager = repositoryManager;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        public  List<Trigger> GetAllTriggersAsync()
        {
            var triggers =  _appStorageService.Triggers.Values.ToList();
            return _mapper.Map<List<Trigger>>(triggers);
        }

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        public async Task<Trigger?> GetTriggerByIdAsync(int id)
        {
            _appStorageService.Triggers.TryGetValue(id, out var trigger);
            return trigger;
        }
        
        
        /// <summary>
        /// 创建一个新的触发器定义
        /// </summary>
        public async Task<Trigger> CreateTriggerAsync(Trigger triggerDto)
        {
            // 1. 验证 DTO (可以在应用层或领域层做)
            // ValidateTriggerDto(triggerDto);

            // 2. 转换 DTO 到实体
            var triggerEntity = _mapper.Map<Trigger>(triggerDto);
            triggerEntity.CreatedAt = DateTime.UtcNow;
            triggerEntity.UpdatedAt = DateTime.UtcNow;

            // 3. 调用仓储保存实体
            var createdTrigger = await _repositoryManager.Triggers.AddAsync(triggerEntity);

            // 4. 转换回 DTO 并返回
            var result = _mapper.Map<Trigger>(createdTrigger);
            
            // 5. 同步更新AppDataStorageService中的Triggers字典
            _appStorageService.Triggers[result.Id] = result;

            return result;
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<Trigger?> UpdateTriggerAsync(int id, Trigger triggerDto)
        {
            // 1. 获取现有实体
            var existingTrigger = await _repositoryManager.Triggers.GetByIdAsync(id);
            if (existingTrigger == null)
                return null;

            // 2. 验证 DTO
            ValidateTriggerDto(triggerDto);

            // 3. 将 DTO 映射到现有实体 (排除不可变字段如 Id, CreatedAt)
            _mapper.Map(triggerDto, existingTrigger, opts => opts.Items["IgnoreIdAndCreatedAt"] = true);
            existingTrigger.UpdatedAt = DateTime.UtcNow;

            // 4. 调用仓储更新实体
            var updatedTrigger = await _repositoryManager.Triggers.UpdateAsync(existingTrigger);
            if (updatedTrigger == null)
                return null;

            // 5. 转换回 DTO 并返回
            var result = _mapper.Map<Trigger>(updatedTrigger);
            
            // 6. 同步更新AppDataStorageService中的Triggers字典
            _appStorageService.Triggers[result.Id] = result;

            return result;
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteTriggerAsync(int id)
        {
            // var result = await _repositoryManager.Triggers.DeleteAsync(id);
            //
            // // 如果删除成功，也从AppDataStorageService中的Triggers字典中移除
            // if (result)
            // {
            //     _appStorageService.Triggers.TryRemove(id, out _);
            // }
            //
            // return result;
            return false;
        }

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        public async Task<List<Trigger>> GetTriggersForVariableAsync(int variableId)
        {
            // var triggers = await _repositoryManager.Triggers.GetByVariableIdAsync(variableId);
            // return _mapper.Map<List<Trigger>>(triggers);
            return null;
        }

        /// <summary>
        /// 异步加载所有触发器数据
        /// </summary>
        public async Task LoadAllTriggersAsync()
        {
            _appStorageService.Triggers.Clear();
            var triggerDefinitions = await  _repositoryManager.Triggers.GetAllAsync();
            foreach (var triggerDefinition in triggerDefinitions)
            {
                _appStorageService.Triggers.TryAdd(triggerDefinition.Id, triggerDefinition);
            }

        }

        /// <summary>
        /// 内部方法：验证 Trigger 的有效性
        /// </summary>
        private void ValidateTriggerDto(Trigger dto)
        {
            // 检查是否至少关联了一个变量
            if (dto.Variables == null || !dto.Variables.Any())
                throw new ArgumentException("触发器必须至少关联一个变量。");

            // 添加必要的验证逻辑
        }
    }
}