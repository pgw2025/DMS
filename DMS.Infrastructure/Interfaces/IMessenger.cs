using System;

namespace DMS.Infrastructure.Interfaces
{
    /// <summary>
    /// 消息传递接口，用于在不同组件之间发送消息
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">要发送的消息</param>
        void Send<T>(T message);

        /// <summary>
        /// 注册消息接收者
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="recipient">接收者</param>
        /// <param name="action">处理消息的动作</param>
        void Register<T>(object recipient, Action<T> action);

        /// <summary>
        /// 取消注册消息接收者
        /// </summary>
        /// <param name="recipient">接收者</param>
        void Unregister(object recipient);

        /// <summary>
        /// 取消注册特定类型消息的接收者
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="recipient">接收者</param>
        void Unregister<T>(object recipient);
    }
}