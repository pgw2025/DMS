using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 消息传递实现，用于在不同组件之间发送消息
    /// </summary>
    public class Messenger : IMessenger
    {
        private readonly ConcurrentDictionary<Type, List<RecipientAction>> _recipients = new ConcurrentDictionary<Type, List<RecipientAction>>();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">要发送的消息</param>
        public void Send<T>(T message)
        {
            var messageType = typeof(T);
            if (_recipients.TryGetValue(messageType, out var actions))
            {
                // 创建副本以避免在迭代时修改集合
                var actionsCopy = new List<RecipientAction>(actions);
                foreach (var action in actionsCopy)
                {
                    action.Action?.DynamicInvoke(message);
                }
            }
        }

        /// <summary>
        /// 注册消息接收者
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="recipient">接收者</param>
        /// <param name="action">处理消息的动作</param>
        public void Register<T>(object recipient, Action<T> action)
        {
            var messageType = typeof(T);
            var recipientAction = new RecipientAction(recipient, action);
            
            _recipients.AddOrUpdate(
                messageType,
                _ => new List<RecipientAction> { recipientAction },
                (_, list) =>
                {
                    list.Add(recipientAction);
                    return list;
                });
        }

        /// <summary>
        /// 取消注册消息接收者
        /// </summary>
        /// <param name="recipient">接收者</param>
        public void Unregister(object recipient)
        {
            foreach (var kvp in _recipients)
            {
                kvp.Value.RemoveAll(r => r.Recipient == recipient);
            }
        }

        /// <summary>
        /// 取消注册特定类型消息的接收者
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="recipient">接收者</param>
        public void Unregister<T>(object recipient)
        {
            var messageType = typeof(T);
            if (_recipients.TryGetValue(messageType, out var actions))
            {
                actions.RemoveAll(r => r.Recipient == recipient);
            }
        }

        /// <summary>
        /// 接收者动作封装类
        /// </summary>
        private class RecipientAction
        {
            public object Recipient { get; }
            public Delegate Action { get; }

            public RecipientAction(object recipient, Delegate action)
            {
                Recipient = recipient;
                Action = action;
            }
        }
    }
}