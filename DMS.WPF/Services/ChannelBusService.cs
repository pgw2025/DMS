using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DMS.WPF.Services
{
    /// <summary>
    /// 标记接口，用于标识可以通过ChannelBusService发送的消息。
    /// </summary>
    public interface IChannelMessage { }

    /// <summary>
    /// 提供基于System.Threading.Channels的消息发布/订阅机制，实现组件解耦。
    /// </summary>
    public class ChannelBusService
    {
        // 使用ConcurrentDictionary存储不同消息类型的Channel
        private readonly ConcurrentDictionary<Type, object> _channels = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 异步发布一条消息到对应的Channel。
        /// </summary>
        /// <typeparam name="TMessage">消息的类型，必须实现IChannelMessage接口。</typeparam>
        /// <param name="message">要发布的消息实例。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示异步操作的Task。</returns>
        public async ValueTask PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
            where TMessage : IChannelMessage
        {
            var channel = GetOrCreateChannel<TMessage>();
            await channel.Writer.WriteAsync(message, cancellationToken);
        }

        /// <summary>
        /// 获取指定消息类型的ChannelReader，用于订阅消息。
        /// </summary>
        /// <typeparam name="TMessage">要订阅的消息类型，必须实现IChannelMessage接口。</typeparam>
        /// <returns>指定消息类型的ChannelReader。</returns>
        public ChannelReader<TMessage> Subscribe<TMessage>()
            where TMessage : IChannelMessage
        {
            var channel = GetOrCreateChannel<TMessage>();
            return channel.Reader;
        }

        /// <summary>
        /// 获取或创建指定消息类型的Channel。
        /// </summary>
        /// <typeparam name="TMessage">消息类型。</typeparam>
        /// <returns>指定消息类型的Channel。</returns>
        private Channel<TMessage> GetOrCreateChannel<TMessage>()
            where TMessage : IChannelMessage
        {
            // 使用GetOrAdd方法确保线程安全地获取或创建Channel
            return (Channel<TMessage>)_channels.GetOrAdd(typeof(TMessage), _ => Channel.CreateUnbounded<TMessage>());
        }
    }
}
