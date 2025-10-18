using DMS.Core.Enums;
using DMS.Core.Models.Triggers;

namespace DMS.Application.Events;

public class TriggerVariableChangedEventArgs : EventArgs
{
    public ActionChangeType ChangeType { get; }
    public TriggerVariable TriggerVariable { get; }

    public TriggerVariableChangedEventArgs(ActionChangeType changeType, TriggerVariable triggerVariable)
    {
        ChangeType = changeType;
        TriggerVariable = triggerVariable;
    }
}