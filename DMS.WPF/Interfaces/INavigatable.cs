// 文件: DMS.WPF/Services/INavigatable.cs

using DMS.Core.Models;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 定义了一个契约，表示ViewModel可以安全地接收导航传入的参数。
/// </summary>
public interface INavigatable
{
    /// <summary>
    /// 当导航到此ViewModel时，由导航服务调用此方法，以传递参数。
    /// </summary>
    /// <param name="parameter">从导航源传递过来的参数对象。</param>
    Task OnNavigatedToAsync(NavigationParameter navigationParameter);
    
    /// <summary>
    /// 当从当前ViewModel导航离开时，由导航服务调用此方法。
    /// </summary>
    /// <param name="nextViewModel">即将导航到的下一个ViewModel。</param>
    Task OnNavigatedFromAsync(NavigationParameter navigationParameter);
    

}
