using DMS.Application.Interfaces;
using DMS.Application.Models;


namespace DMS.Application.Services.Processors;

/// <summary>
/// 一个简单的数据处理器实现，用于演示。
/// 其主要功能是记录接收到的变量数据的名称和值。
/// </summary>
public class LoggingProcessor : IVariableProcessor
{
    public LoggingProcessor()
    {
    }

    public Task ProcessAsync(VariableContext context)
    {
        // NlogHelper.Info($"处理数据: {context.Data.Name}, 值: {context.Data.DataValue}");
        return Task.CompletedTask;
    }
}
