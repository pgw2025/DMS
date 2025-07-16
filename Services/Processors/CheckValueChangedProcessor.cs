using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.Services.Processors;

public class CheckValueChangedProcessor : IVariableDataProcessor
{
    private readonly DataServices _dataServices;

    public CheckValueChangedProcessor(DataServices dataServices)
    {
        _dataServices = dataServices;
    }
    public Task ProcessAsync(VariableDataContext context)
    {
        VariableData newVariable = context.Data;
        if (!_dataServices.AllVariables.TryGetValue(newVariable.Id, out VariableData oldVariable))
        {
            NlogHelper.Warn($"检查变量值是否改变时在_dataServices.AllVariables中找不到Id:{newVariable.Id},Name:{newVariable.Name}的变量。");
            context.IsHandled = true;
            return Task.CompletedTask;
        }

        if (newVariable.DataValue == oldVariable.DataValue)
        {
            // 值没有变化，直接完成
            context.IsHandled = true;
        }
        
        // 在这里处理 context.Data
        return Task.CompletedTask;
    }
}