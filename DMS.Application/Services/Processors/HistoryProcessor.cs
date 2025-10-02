using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

public class HistoryProcessor : IVariableProcessor, IDisposable
{
    private const int BATCH_SIZE = 50; // 批量写入的阈值
    private const int TIMER_INTERVAL_MS = 30 * 1000; // 30秒

    private readonly ConcurrentQueue<VariableHistory> _queue = new();
    private readonly Timer _timer;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILogger<HistoryProcessor> _logger;

    public HistoryProcessor(IRepositoryManager repositoryManager, ILogger<HistoryProcessor> logger)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;

        _timer = new Timer(async _ => await FlushQueueToDatabase(), null, Timeout.Infinite, Timeout.Infinite);
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS); // 启动定时器
        
        _logger.LogInformation("HistoryProcessor 初始化，批量大小 {BatchSize}，定时器间隔 {TimerInterval}ms", BATCH_SIZE, TIMER_INTERVAL_MS);
    }

    public async Task ProcessAsync(VariableContext context)
    {
        // 只有当数据需要保存时才记录历史
        if (!context.Data.IsHistoryEnabled) // 如果数据已经被其他处理器处理过或者不需要保存，则跳过
        {
            // _logger.LogDebug("变量 {VariableName} (ID: {VariableId}) 历史记录已禁用，跳过处理", context.Data.Name, context.Data.Id);
            return;
        }

        // 将 VariableDto 转换为 VariableHistory
        var historyData = new VariableHistory
        {
            VariableId = context.Data.Id,
            Value = context.Data.DisplayValue?.ToString() ?? string.Empty,
            Timestamp = DateTime.Now // 记录当前时间
        };

        _queue.Enqueue(historyData);
        _logger.LogDebug("变量 {VariableName} (ID: {VariableId}) 历史数据已入队，队列数量: {QueueCount}", context.Data.Name, context.Data.Id, _queue.Count);

        if (_queue.Count >= BATCH_SIZE)
        {
            _logger.LogInformation("达到批量大小 ({BatchSize})，正在刷新队列到数据库", BATCH_SIZE);
            await FlushQueueToDatabase();
        }
    }

    private async Task FlushQueueToDatabase()
    {
        // 停止定时器，防止在写入过程中再次触发
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.LogDebug("数据库刷新期间定时器已停止");

        var itemsToProcess = new List<VariableHistory>();
        while (_queue.TryDequeue(out var item))
        {
            itemsToProcess.Add(item);
        }

        if (itemsToProcess.Any())
        {
            try
            {
                await _repositoryManager.VariableHistories.AddBatchAsync(itemsToProcess);
                _logger.LogInformation("成功插入 {Count} 条变量历史记录到数据库", itemsToProcess.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量插入 {Count} 条变量历史记录时发生错误: {ErrorMessage}", itemsToProcess.Count, ex.Message);
                // 错误处理：可以将未成功插入的数据重新放回队列，或者记录到日志中以便后续处理
                // 为了简单起见，这里不重新入队，避免无限循环
            }
        }
        else
        {
            _logger.LogDebug("队列中没有需要处理的项目");
        }

        // 重新启动定时器
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS);
        _logger.LogDebug("定时器已重启，间隔 {TimerInterval}ms", TIMER_INTERVAL_MS);
    }

    public void Dispose()
    {
        _logger.LogInformation("正在释放 HistoryProcessor，刷新队列中剩余的 {Count} 个项目", _queue.Count);
        _timer?.Dispose();
        // 在 Dispose 时，尝试将剩余数据写入数据库
        FlushQueueToDatabase().Wait();
        _logger.LogInformation("HistoryProcessor 已释放");
    }
}