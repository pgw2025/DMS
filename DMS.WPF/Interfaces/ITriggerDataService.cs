using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 触发器数据服务接口。
/// </summary>
public interface ITriggerDataService
{
    /// <summary>
    /// 加载所有触发器数据。
    /// </summary>
    void LoadAllTriggers();

    /// <summary>
    /// 添加触发器。
    /// </summary>
    Task<TriggerItem> AddTrigger(TriggerItem dto);

    /// <summary>
    /// 删除触发器。
    /// </summary>
    Task<bool> DeleteTrigger(TriggerItem trigger);

    /// <summary>
    /// 更新触发器。
    /// </summary>
    Task<bool> UpdateTrigger(TriggerItem trigger);
}