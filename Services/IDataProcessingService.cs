using System.Threading.Tasks;
using PMSWPF.Models;

namespace PMSWPF.Services;

/// <summary>
/// 定义了数据处理服务的接口。
/// 该服务负责管理数据处理队列和处理器链。
/// </summary>
public interface IDataProcessingService
{
    /// <summary>
    /// 向处理链中添加一个数据处理器。
    /// </summary>
    /// <param name="processor">要添加的数据处理器实例。</param>
    void AddProcessor(IVariableProcessor processor);

    /// <summary>
    /// 将一个变量数据项异步推入处理队列。
    /// </summary>
    /// <param name="data">要入队的变量数据。</param>
    /// <returns>一个表示入队操作的 ValueTask。</returns>
    ValueTask EnqueueAsync(Variable data);
}
