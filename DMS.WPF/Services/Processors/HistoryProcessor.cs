using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Services;
using Microsoft.Extensions.Logging;

namespace DMS.WPF.Services.Processors;

public class HistoryProcessor : IVariableProcessor, IDisposable
{
    private const int BATCH_SIZE = 50; // 批量写入的阈值
    private const int TIMER_INTERVAL_MS = 30 * 1000; // 30秒

    // private readonly ConcurrentQueue<DbVariableHistory> _queue = new();
    private readonly Timer _timer;

    public HistoryProcessor()
    {

        _timer = new Timer(async _ => await FlushQueueToDatabase(), null, Timeout.Infinite, Timeout.Infinite);
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS); // 启动定时器
    }

    public async Task ProcessAsync(VariableContext context)
    {
        // 只有当数据发生变化时才记录历史
        // if (!context.Data.IsSave) // 如果数据已经被其他处理器处理过或者不需要保存，则跳过
        // {
        //     return;
        // }
        //
        // // 将 Variable 转换为 DbVariableHistory
        // var historyData = new DbVariableHistory
        // {
        //     Name = context.Data.Name,
        //     NodeId = context.Data.NodeId,
        //     DataValue = context.Data.DataValue,
        //     VariableId = context.Data.Id,
        //     Timestamp = DateTime.Now // 记录当前时间
        // };
        //
        // _queue.Enqueue(historyData);
        //
        // if (_queue.Count >= BATCH_SIZE)
        // {
        //     await FlushQueueToDatabase();
        // }
    }

    private async Task FlushQueueToDatabase()
    {
        // 停止定时器，防止在写入过程中再次触发
        // _timer.Change(Timeout.Infinite, Timeout.Infinite);
        //
        // var itemsToProcess = new List<DbVariableHistory>();
        // while (_queue.TryDequeue(out var item))
        // {
        //     itemsToProcess.Add(item);
        // }
        //
        // if (itemsToProcess.Any())
        // {
        //     try
        //     {
        //         using var db = DbContext.GetInstance();
        //         await db.Insertable(itemsToProcess).ExecuteCommandAsync();
        //         NlogHelper.Info($"成功批量插入 {itemsToProcess.Count} 条历史变量数据。");
        //     }
        //     catch (Exception ex)
        //     {
        //         NlogHelper.Error( $"批量插入历史变量数据时发生错误: {ex.Message}",ex);
        //         // 错误处理：可以将未成功插入的数据重新放回队列，或者记录到日志中以便后续处理
        //         // 为了简单起见，这里不重新入队，避免无限循环
        //     }
        // }

        // 重新启动定时器
        _timer.Change(TIMER_INTERVAL_MS, TIMER_INTERVAL_MS);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        // 在 Dispose 时，尝试将剩余数据写入数据库
        FlushQueueToDatabase().Wait(); 
    }
}