using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.Services.Processors;

/// <summary>
/// 一个简单的数据处理器实现，用于演示。
/// 其主要功能是记录接收到的变量数据的名称和值。
/// </summary>
public class LoggingDataProcessor : IVariableDataProcessor
{

    /// <summary>
    /// 构造函数，注入日志记录器。
    /// </summary>
    /// <param name="logger">日志记录器实例。</param>
    public LoggingDataProcessor()
    {
    }

    /// <summary>
    /// 实现处理逻辑，此处为记录日志。
    /// </summary>
    /// <param name="data">要处理的变量数据。</param>
    /// <returns>一个表示完成的异步任务。</returns>
    public Task ProcessAsync(VariableData data)
    {
        // 使用日志记录器输出变量的名称和值
        NlogHelper.Info($"处理数据: {data.Name}, 值: {data.DataValue}");
        // 由于此操作是同步的，直接返回一个已完成的任务。
        return Task.CompletedTask;
    }
}
