using System.Collections.Concurrent;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

/// <summary>
/// 负责将变量的当前值批量更新到数据库。
/// </summary>
public class UpdateDbVariableProcessor : IVariableProcessor, IDisposable
{
    private const int BATCH_SIZE = 50; // 批量更新的阈值
    private const int TIMER_INTERVAL_MS = 30 * 1000; // 30秒

    private readonly ConcurrentQueue<VariableDto> _queue = new();
    private readonly Timer _timer;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILogger<UpdateDbVariableProcessor> _logger;
    private readonly IMapper _mapper;

    public UpdateDbVariableProcessor(IRepositoryManager repositoryManager, ILogger<UpdateDbVariableProcessor> logger, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
        _mapper = mapper;

        _timer = new Timer(async _ => await FlushQueueToDatabase(), null, Timeout.Infinite, Timeout.Infinite);
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS); // 启动定时器
        
        _logger.LogInformation("UpdateDbVariableProcessor 初始化，批量大小 {BatchSize}，定时器间隔 {TimerInterval}ms", BATCH_SIZE, TIMER_INTERVAL_MS);
    }

    public async Task ProcessAsync(VariableContext context)
    {

        _queue.Enqueue(context.Data);
        
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

        var itemsToProcess = new List<VariableDto>();
        while (_queue.TryDequeue(out var item))
        {
            itemsToProcess.Add(item);
        }

        if (itemsToProcess.Any())
        {
            // 去重：对于同一个变量，我们只关心其在本次批次中的最后一次更新值
            var uniqueItems = itemsToProcess
                .GroupBy(v => v.Id)
                .Select(g => g.Last())
                .ToList();

            try
            {
                // **依赖于仓储层实现真正的批量更新**
                var variableModels = _mapper.Map<IEnumerable<Variable>>(uniqueItems);
                await _repositoryManager.Variables.UpdateBatchAsync(variableModels);
                _logger.LogInformation("成功批量更新 {Count} 条变量记录到数据库", uniqueItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新 {Count} 条变量记录时发生错误: {ErrorMessage}", uniqueItems.Count, ex.Message);
                // 错误处理策略：可以考虑将未成功的项重新入队，或记录到死信队列
            }
        }
        
        // 重新启动定时器
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS);
    }

    public void Dispose()
    {
        _logger.LogInformation("正在释放 UpdateDbVariableProcessor，刷新队列中剩余的 {Count} 个项目", _queue.Count);
        _timer?.Dispose();
        // 在 Dispose 时，尝试将剩余数据写入数据库
        FlushQueueToDatabase().Wait();
        _logger.LogInformation("UpdateDbVariableProcessor 已释放");
    }
}