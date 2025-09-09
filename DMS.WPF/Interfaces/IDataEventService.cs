using DMS.Message;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 数据事件服务接口。
/// </summary>
public interface IDataEventService
{
    /// <summary>
    /// 处理LoadMessage消息。
    /// </summary>
    Task Receive(LoadMessage message);
}