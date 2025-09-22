using DMS.Application.DTOs;

namespace DMS.Application.Services.Management
{
    /// <summary>
    /// 触发器管理服务接口 (负责 CRUD 操作)
    /// </summary>
    public interface ITriggerManagementService
    {
        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        /// <returns>触发器定义列表</returns>
        Task<List<TriggerDefinitionDto>> GetAllTriggersAsync();

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        /// <param name="id">触发器 ID</param>
        /// <returns>触发器定义 DTO，如果未找到则返回 null</returns>
        Task<TriggerDefinitionDto?> GetTriggerByIdAsync(int id);

        /// <summary>
        /// 创建一个新的触发器定义
        /// </summary>
        /// <param name="triggerDto">要创建的触发器定义 DTO</param>
        /// <returns>创建成功的触发器定义 DTO</returns>
        Task<TriggerDefinitionDto> CreateTriggerAsync(TriggerDefinitionDto triggerDto);

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        /// <param name="id">要更新的触发器 ID</param>
        /// <param name="triggerDto">包含更新信息的触发器定义 DTO</param>
        /// <returns>更新后的触发器定义 DTO，如果未找到则返回 null</returns>
        Task<TriggerDefinitionDto?> UpdateTriggerAsync(int id, TriggerDefinitionDto triggerDto);

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        /// <param name="id">要删除的触发器 ID</param>
        /// <returns>删除成功返回 true，否则返回 false</returns>
        Task<bool> DeleteTriggerAsync(int id);

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        /// <param name="variableId">变量 ID</param>
        /// <returns>该变量关联的触发器定义列表</returns>
        Task<List<TriggerDefinitionDto>> GetTriggersForVariableAsync(int variableId);
    }
}