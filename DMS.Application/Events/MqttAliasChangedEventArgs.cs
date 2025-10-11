using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Events;

public class MqttAliasChangedEventArgs : EventArgs
{
    public ActionChangeType ChangeType { get; }
    public MqttAlias MqttAlias { get; }
    public MqttAliasPropertyType PropertyType { get; }

    public MqttAliasChangedEventArgs(ActionChangeType changeType, MqttAlias mqttAlias, MqttAliasPropertyType propertyType = MqttAliasPropertyType.All)
    {
        ChangeType = changeType;
        MqttAlias = mqttAlias;
        PropertyType = propertyType;
    }
}
