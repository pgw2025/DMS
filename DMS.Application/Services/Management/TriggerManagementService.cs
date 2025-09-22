using AutoMapper;
using DMS.Application.DTOs;
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
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public TriggerManagementService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        public async Task<List<TriggerDefinitionDto>> GetAllTriggersAsync()
        {
            var triggers = await _repositoryManager.Triggers.GetAllAsync();
            return _mapper.Map<List<TriggerDefinitionDto>>(triggers);
        }

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        public async Task<TriggerDefinitionDto?> GetTriggerByIdAsync(int id)
        {
            var trigger = await _repositoryManager.Triggers.GetByIdAsync(id);
            return trigger != null ? _mapper.Map<TriggerDefinitionDto>(trigger) : null;
        }

        /// <summary>
        /// 创建一个新的触发器定义
        /// </summary>
        public async Task<TriggerDefinitionDto> CreateTriggerAsync(TriggerDefinitionDto triggerDto)
        {
            // 1. 验证 DTO (可以在应用层或领域层做)
            ValidateTriggerDto(triggerDto);

            // 2. 转换 DTO 到实体
            var triggerEntity = _mapper.Map<TriggerDefinition>(triggerDto);
            triggerEntity.CreatedAt = DateTime.UtcNow;
            triggerEntity.UpdatedAt = DateTime.UtcNow;

            // 3. 调用仓储保存实体
            var createdTrigger = await _repositoryManager.Triggers.AddAsync(triggerEntity);

            // 4. 转换回 DTO 并返回
            return _mapper.Map<TriggerDefinitionDto>(createdTrigger);
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<TriggerDefinitionDto?> UpdateTriggerAsync(int id, TriggerDefinitionDto triggerDto)
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
            return _mapper.Map<TriggerDefinitionDto>(updatedTrigger);
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteTriggerAsync(int id)
        {
            return await _repositoryManager.Triggers.DeleteAsync(id);
        }

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        public async Task<List<TriggerDefinitionDto>> GetTriggersForVariableAsync(int variableId)
        {
            var triggers = await _repositoryManager.Triggers.GetByVariableIdAsync(variableId);
            return _mapper.Map<List<TriggerDefinitionDto>>(triggers);
        }

        /// <summary>
        /// 内部方法：验证 TriggerDefinitionDto 的有效性
        /// </summary>
        private void ValidateTriggerDto(TriggerDefinitionDto dto)
        {
            // 检查是否至少关联了一个变量
            if (dto.VariableIds == null || !dto.VariableIds.Any())
                throw new ArgumentException("触发器必须至少关联一个变量。");

            // 添加必要的验证逻辑
            switch (dto.Condition)
            {
                case ConditionType.GreaterThan:
                case ConditionType.LessThan:
                case ConditionType.EqualTo:
                case ConditionType.NotEqualTo:
                    if (!dto.Threshold.HasValue)
                        throw new ArgumentException($"{dto.Condition} requires Threshold.");
                    break;
                case ConditionType.InRange:
                case ConditionType.OutOfRange:
                    if (!dto.LowerBound.HasValue || !dto.UpperBound.HasValue)
                        throw new ArgumentException($"{dto.Condition} requires LowerBound and UpperBound.");
                    if (dto.LowerBound > dto.UpperBound)
                        throw new ArgumentException("LowerBound must be less than or equal to UpperBound.");
                    break;
            }
        }
    }
}