using System.Threading.Tasks;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

public class UpdateDbVariableProcessor : IVariableProcessor
{
    private readonly ILogger<UpdateDbVariableProcessor> _logger;

    public UpdateDbVariableProcessor(ILogger<UpdateDbVariableProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(VariableContext context)
    {
        try
        {
            // 假设 DataServices 有一个方法来更新 Variable
            // await _dataServices.UpdateVariableAsync(context.Data);
            // _logger.LogInformation($"数据库变量 {context.Data.Name} 更新成功，值为: {context.Data.DataValue}");

            // if (!_dataServices.AllVariables.TryGetValue(context.Data.Id, out Variable oldVariable))
            // {
            //     _logger.LogWarning($"数据库更新完成修改变量值是否改变时在_dataServices.AllVariables中找不到Id:{context.Data.Id},Name:{context.Data.Name}的变量。");
            //     context.IsHandled = true;
            // }
            // oldVariable.DataValue = context.Data.DataValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新数据库变量 {context.Data.Name} 失败: {ex.Message}");
        }
    }
}