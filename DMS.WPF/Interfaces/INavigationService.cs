// 文件: DMS.WPF/Services/INavigationService.cs

using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 定义了应用程序的导航服务接口。
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 导航到由唯一键标识的视图，并传递一个参数。
    /// </summary>
    /// <param name="viewKey">在DI容器中注册的目标视图的唯一键（通常是ViewModel的名称）。</param>
    /// <param name="parameter">要传递给目标ViewModel的参数。</param>
    Task NavigateToAsync(MenuItemViewModel menu);

}
