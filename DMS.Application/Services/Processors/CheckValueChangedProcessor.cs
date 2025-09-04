using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Application.Models;

namespace DMS.Application.Services.Processors;

public class CheckValueChangedProcessor : IVariableProcessor
{

    public CheckValueChangedProcessor()
    {
    }
    public Task ProcessAsync(VariableContext context)
    {
        // Variable newVariable = context.Data;
        // if (!_dataServices.AllVariables.TryGetValue(newVariable.Id, out Variable oldVariable))
        // {
        //     NlogHelper.Warn($"检查变量值是否改变时在_dataServices.AllVariables中找不到Id:{newVariable.Id},Name:{newVariable.Name}的变量。");
        //     context.IsHandled = true;
        //     return Task.CompletedTask;
        // }

        // if (newVariable.DataValue == oldVariable.DataValue)
        // {
        //     // 值没有变化，直接完成
        //     context.IsHandled = true;
        // }
        //
        // 在这里处理 context.Data
        return Task.CompletedTask;
    }
}