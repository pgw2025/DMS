using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Events;

namespace DMS.Application.Services.Processors;

public class UpdateViewProcessor: IVariableProcessor
{
    private readonly IEventService _eventService;

    public UpdateViewProcessor(IEventService eventService)
    {
        _eventService = eventService;
    }
    
    public async Task ProcessAsync(VariableContext context)
    {
        // 触发变量值变更事件
        var eventArgs = new VariableValueChangedEventArgs(
            context.Data.Id,
            context.Data.Name,
            context.Data.DataValue,
            context.NewValue.ToString()??"",
            DateTime.Now);

        _eventService.RaiseVariableValueChanged(this,eventArgs);
        
    }
}