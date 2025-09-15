using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Application.Services.Triggers;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

public class TriggerProcessor : IVariableProcessor
{
    private readonly ITriggerEvaluationService _triggerEvaluationService;
    private readonly ILogger<TriggerProcessor> _logger;

    public TriggerProcessor(ITriggerEvaluationService triggerEvaluationService, ILogger<TriggerProcessor> logger)
    {
        _triggerEvaluationService = triggerEvaluationService;
        _logger = logger;
    }

    public async Task ProcessAsync(VariableContext context)
    {
        // try
        // {
        //     // 调用触发器评估服务来评估与变量关联的所有激活状态的触发器
        //     await _triggerEvaluationService.EvaluateTriggersAsync(context.Data.Id, context.Data.DataValue);
        //     
        //     _logger.LogDebug("触发器评估完成，变量 ID: {VariableId}", context.Data.Id);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "评估变量 {VariableId} 的触发器时发生错误", context.Data.Id);
        //     // 不抛出异常，避免影响其他处理器
        // }
    }
}