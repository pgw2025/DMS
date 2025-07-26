using DMS.Core.Models;

namespace DMS.Core.Models;

public class VariableMqtt
{
    public Variable Variable { get; set; }
    public MqttServer Mqtt { get; set; }
    public int MqttId { get; set; }
}