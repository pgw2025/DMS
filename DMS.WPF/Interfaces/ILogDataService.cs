using System.Collections.ObjectModel;
using DMS.Application.Events;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 日志数据服务接口。
/// </summary>
public interface ILogDataService
{

    /// <summary>
    /// 处理日志变更事件。
    /// </summary>
    void OnNlogChanged(object sender, NlogChangedEventArgs e);

    void LoadAllLog();
}