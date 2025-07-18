using System.ComponentModel;

namespace DMS.Core.Enums;

public enum OpcUaUpdateType
{
    [Description("OpcUa轮询")]
    OpcUaPoll,
    [Description("OpcUa订阅")]
    OpcUaSubscription
}