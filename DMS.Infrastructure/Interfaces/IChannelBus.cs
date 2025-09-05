using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    /// <summary>
    /// 通道总线接口，用于在不同组件之间传递数据
    /// </summary>
    public interface IChannelBus
    {
        /// <summary>
        /// 获取指定名称的通道写入器
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <returns>通道写入器</returns>
        ChannelWriter<T> GetWriter<T>(string channelName);

        /// <summary>
        /// 获取指定名称的通道读取器
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <returns>通道读取器</returns>
        ChannelReader<T> GetReader<T>(string channelName);

        /// <summary>
        /// 创建指定名称的通道
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        /// <param name="capacity">通道容量</param>
        void CreateChannel<T>(string channelName, int capacity = 100);

        /// <summary>
        /// 关闭指定名称的通道
        /// </summary>
        /// <typeparam name="T">通道中传递的数据类型</typeparam>
        /// <param name="channelName">通道名称</param>
        void CloseChannel<T>(string channelName);
    }
}