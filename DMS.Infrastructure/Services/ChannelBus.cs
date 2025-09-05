using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 通道总线实现，用于在不同组件之间传递数据
    /// </summary>
    public class ChannelBus : IChannelBus
    {
        private readonly ConcurrentDictionary<string, object> _channels = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 获取指定名称的通道写入器
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <returns>通道写入器</returns>
        public ChannelWriter<T> GetWriter<T>(string channelName)
        {
            var channel = GetOrCreateChannel<T>(channelName);
            return channel.Writer;
        }

        /// <summary>
        /// 获取指定名称的通道读取器
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <returns>通道读取器</returns>
        public ChannelReader<T> GetReader<T>(string channelName)
        {
            var channel = GetOrCreateChannel<T>(channelName);
            return channel.Reader;
        }

        /// <summary>
        /// 创建指定名称的通道
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <param name="capacity">通道容量</param>
        public void CreateChannel<T>(string channelName, int capacity = 100)
        {
            _channels.GetOrAdd(channelName, _ => Channel.CreateBounded<T>(capacity));
        }

        /// <summary>
        /// 关闭指定名称的通道
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        public void CloseChannel<T>(string channelName)
        {
            if (_channels.TryRemove(channelName, out var channel))
            {
                if (channel is Channel<T> typedChannel)
                {
                    typedChannel.Writer.Complete();
                }
            }
        }

        /// <summary>
        /// 获取或创建指定名称的通道
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <returns>通道</returns>
        private Channel<T> GetOrCreateChannel<T>(string channelName)
        {
            return (Channel<T>)_channels.GetOrAdd(channelName, _ => Channel.CreateBounded<T>(100));
        }
    }
}