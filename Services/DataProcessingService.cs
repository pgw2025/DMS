using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMSWPF.Models;

namespace PMSWPF.Services;

/// <summary>
/// 核心数据处理服务，作为后台服务运行。
/// 它维护一个无界通道（Channel）作为处理队列，并按顺序执行已注册的数据处理器。
/// </summary>
public class DataProcessingService : BackgroundService, IDataProcessingService
{
    private readonly ILogger<DataProcessingService> _logger;
    // 使用 Channel 作为高性能的生产者/消费者队列
    private readonly Channel<VariableData> _queue;
    // 存储数据处理器的链表
    private readonly List<IVariableDataProcessor> _processors;

    /// <summary>
    /// 构造函数，注入日志记录器。
    /// </summary>
    /// <param name="logger">日志记录器实例。</param>
    public DataProcessingService(ILogger<DataProcessingService> logger)
    {
        _logger = logger;
        // 创建一个无边界的 Channel，允许生产者快速写入而不会被阻塞。
        _queue = Channel.CreateUnbounded<VariableData>();
        _processors = new List<IVariableDataProcessor>();
    }

    /// <summary>
    /// 向处理链中添加一个数据处理器。
    /// 处理器将按照添加的顺序执行。
    /// </summary>
    /// <param name="processor">要添加的数据处理器实例。</param>
    public void AddProcessor(IVariableDataProcessor processor)
    {
        _processors.Add(processor);
    }

    /// <summary>
    /// 将一个变量数据项异步推入处理队列。
    /// </summary>
    /// <param name="data">要入队的变量数据。</param>
    public async ValueTask EnqueueAsync(VariableData data)
    {
        if (data == null)
        {
            return;
        }

        // 将数据项写入 Channel，供后台服务处理。
        await _queue.Writer.WriteAsync(data);
    }

    /// <summary>
    /// 后台服务的核心执行逻辑。
    /// 此方法会持续运行，从队列中读取数据并交由处理器链处理。
    /// </summary>
    /// <param name="stoppingToken">用于通知服务停止的取消令牌。</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("数据处理服务已启动。");

        // 当服务未被请求停止时，持续循环
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 从队列中异步读取一个数据项，如果队列为空，则等待。
                var data = await _queue.Reader.ReadAsync(stoppingToken);

                // 依次调用处理链中的每一个处理器
                foreach (var processor in _processors)
                {
                    await processor.ProcessAsync(data);
                }
            }
            catch (OperationCanceledException)
            {
                // 当 stoppingToken 被触发时，ReadAsync 会抛出此异常，属正常停止流程，无需处理。
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理变量数据时发生错误。");
            }
        }

        _logger.LogInformation("数据处理服务已停止。");
    }
}
