using System.Threading.Tasks;
using PMSWPF.Models;

namespace PMSWPF.Services;

/// <summary>
/// 定义了变量数据处理器的通用接口。
/// 任何需要加入数据处理链的类都必须实现此接口。
/// </summary>
public interface IVariableProcessor
{
    /// <summary>
    /// 异步处理单个变量数据。
    /// </summary>
    /// <param name="data">要处理的变量数据。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    Task ProcessAsync(VariableContext context);
}
