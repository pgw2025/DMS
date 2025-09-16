namespace DMS.Core.Events;

public class VariablesActiveChangedEventArgs: EventArgs
{

    public List<int> VariableIds { get; }
    
    public int DeviceId{get;}

    public bool NewStatus { get; }



    public VariablesActiveChangedEventArgs(List<int> variableIds,int deviceId, bool newStatus)
    {
        VariableIds = variableIds;
        DeviceId=deviceId;
        NewStatus = newStatus;
    }
}