using DMS.Application.Events;
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
            context.Data,
            context.Data.DataValue);

        _eventService.RaiseVariableValueChanged(this, eventArgs);
        
    }
}