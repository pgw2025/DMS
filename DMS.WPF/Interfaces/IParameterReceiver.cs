namespace DMS.WPF.Interfaces;

/// <summary>
/// 定义了可以接收导航参数的接口
/// </summary>
public interface IParameterReceiver
{
    /// <summary>
    /// 接收导航参数
    /// </summary>
    /// <param name="parameter">传递的参数</param>
    void ReceiveParameter(object parameter);
}