using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Application.Services.Triggers;
using DMS.Core.Enums;
using DMS.Core.Events;
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
        private readonly ITriggerAppService _triggerAppService;
        private readonly IMapper _mapper;
        private readonly IEventService _eventService;

        public TriggerManagementService(IAppStorageService appStorageService, ITriggerAppService triggerAppService, IMapper mapper, IEventService eventService)
        {
            _appStorageService = appStorageService;
            _triggerAppService = triggerAppService;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _eventService = eventService;
        }

        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        public List<Trigger> GetAllTriggersAsync()
        {
            var triggers = _appStorageService.Triggers.Values.ToList();
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
        public async Task<Trigger> AddTriggerAsync(Trigger trigger)
        {
            var createdTrigger = await _triggerAppService.AddTriggerAsync(trigger);

            // 创建成功后，将触发器添加到内存中
            if (createdTrigger != null)
            {
                if (_appStorageService.Triggers.TryAdd(createdTrigger.Id, createdTrigger))
                {
                    _eventService.RaiseTriggerChanged(this, new TriggerChangedEventArgs(DataChangeType.Added, createdTrigger));
                }
            }

            return createdTrigger;
        }

        /// <summary>
        /// 创建触发器及其关联菜单
        /// </summary>
        public async Task<CreateTriggerWithMenuDto> CreateTriggerWithMenuAsync(CreateTriggerWithMenuDto dto)
        {
            var result = await _triggerAppService.CreateTriggerWithMenuAsync(dto);

            // 创建成功后，将触发器添加到内存中
            if (result is null || result.Trigger is null)
            {
                return null;
            }

            if (_appStorageService.Triggers.TryAdd(result.Trigger.Id, result.Trigger))
            {
                _eventService.RaiseTriggerChanged(this, new TriggerChangedEventArgs(DataChangeType.Added, result.Trigger));
            }

            if (_appStorageService.Menus.TryAdd(result.TriggerMenu.Id, result.TriggerMenu))
            {
                _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Added, result.TriggerMenu));
            }


            return result;
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<int> UpdateTriggerAsync(Trigger trigger)
        {

            // 4. 调用仓储更新实体
            var res = await _triggerAppService.UpdateTriggerAsync(trigger);
            if (res == 0)
                return res;

            // 6. 同步更新AppDataStorageService中的Triggers字典
            if (_appStorageService.Triggers.TryGetValue(trigger.Id, out var memTrigger))
            {
                _mapper.Map(trigger, memTrigger);
            }


            return res;
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteTriggerAsync(int id)
        {

            // 如果删除成功，也从AppDataStorageService中的Triggers字典中移除
            if (await _triggerAppService.DeleteTriggerByIdAsync(id))
            {
                _appStorageService.Triggers.TryRemove(id, out _);
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        public async Task<List<Trigger>> GetTriggersForVariableAsync(int variableId)
        {
            // var triggers = await _repositoryManager.Triggers.GetByVariableIdAsync(variableId);
            // return _mapper.Map<List<TriggerMenu>>(triggers);
            return null;
        }

        /// <summary>
        /// 异步加载所有触发器数据
        /// </summary>
        public async Task LoadAllTriggersAsync()
        {
            _appStorageService.Triggers.Clear();
            var triggerDefinitions = await _triggerAppService.GetAllTriggersAsync();
            foreach (var triggerDefinition in triggerDefinitions)
            {
                _appStorageService.Triggers.TryAdd(triggerDefinition.Id, triggerDefinition);
            }

        }

        /// <summary>
        /// 内部方法：验证 TriggerMenu 的有效性
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