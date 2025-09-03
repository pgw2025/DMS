using System.Threading.Channels;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using Microsoft.Extensions.Hosting;

namespace DMS.Services;

/// <summary>
/// 核心数据处理服务，作为后台服务运行。
/// 它维护一个无界通道（Channel）作为处理队列，并按顺序执行已注册的数据处理器。
/// </summary>
public class DataProcessingService : BackgroundService, IDataProcessingService
{
    // 使用 Channel 作为高性能的生产者/消费者队列
    private readonly Channel<VariableContext> _queue;

    // 存储数据处理器的链表
    private readonly List<IVariableProcessor> _processors;

    /// <summary>
    /// 构造函数，注入日志记录器。
    /// </summary>
    /// <param name="logger">日志记录器实例。</param>
    public DataProcessingService()
    {
        // 创建一个无边界的 Channel，允许生产者快速写入而不会被阻塞。
        _queue = Channel.CreateUnbounded<VariableContext>();
        _processors = new List<IVariableProcessor>();
    }

    /// <summary>
    /// 向处理链中添加一个数据处理器。
    /// 处理器将按照添加的顺序执行。
    /// </summary>
    /// <param name="processor">要添加的数据处理器实例。</param>
    public void AddProcessor(IVariableProcessor processor)
    {
        _processors.Add(processor);
    }

    /// <summary>
    /// 将一个变量数据项异步推入处理队列。
    /// </summary>
    /// <param name="data">要入队的变量数据。</param>
    public async ValueTask EnqueueAsync(Variable data)
    {
        if (data == null)
        {
            return;
        }

        var context = new VariableContext(data);
        // 将数据项写入 Channel，供后台服务处理。
        await _queue.Writer.WriteAsync(context);
    }

    /// <summary>
    /// 后台服务的核心执行逻辑。
    /// 此方法会持续运行，从队列中读取数据并交由处理器链处理。
    /// </summary>
    /// <param name="stoppingToken">用于通知服务停止的取消令牌。</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NlogHelper.Info("数据处理服务已启动。");

        // 当服务未被请求停止时，持续循环
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 从队列中异步读取一个数据项，如果队列为空，则等待。
                var context = await _queue.Reader.ReadAsync(stoppingToken);

                // 依次调用处理链中的每一个处理器
                foreach (var processor in _processors)
                {
                    if (context.IsHandled)
                    {
                        // NlogHelper.Info($"{context.Data.Name}的数据处理已短路，跳过后续处理器。");
                        break; // 短路，跳过后续处理器
                    }

                    await processor.ProcessAsync(context);
                }
            }
            catch (OperationCanceledException)
            {
                // 当 stoppingToken 被触发时，ReadAsync 会抛出此异常，属正常停止流程，无需处理。
            }
            catch (Exception ex)
            {
                NlogHelper.Error($"处理变量数据时发生错误:{ex.Message}", ex);
            }
        }

        NlogHelper.Info("数据处理服务已停止。");
    }
}