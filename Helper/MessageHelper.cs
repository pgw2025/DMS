using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Enums;
using PMSWPF.Message;
using PMSWPF.ViewModels;

namespace PMSWPF.Helper;

public class MessageHelper
{
    public static void Send<T>(T message) where T : class
    {
        WeakReferenceMessenger.Default.Send<T>(message);
    }
    /// <summary>
    /// 发送加载消息
    /// </summary>
    /// <param name="loadType">加载的类型，如菜单</param>
    public static void SendLoadMessage(LoadTypes loadType)
    {
        WeakReferenceMessenger.Default.Send<LoadMessage>(new LoadMessage(loadType));
    }
    /// <summary>
    /// 发送导航消息
    /// </summary>
    /// <param name="vm">导航View的ViewModel</param>
    /// <param name="param">带的参数</param>
    public static void SendNavgatorMessage(ViewModelBase vm)
    {
        WeakReferenceMessenger.Default.Send<NavgatorMessage>(new NavgatorMessage(vm));
    }
}