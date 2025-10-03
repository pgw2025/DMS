using DMS.Core.Enums;

namespace DMS.Core.Models;

public class NavigationParameter
{
    
    public string TargetViewKey { get; set; }
    
    public NavigationType TargetType { get; set; }
    
    public int TargetId { get; set; }

    public NavigationParameter(string targetViewKey, int targetId=0,NavigationType targetType=NavigationType.None )
    {
        TargetViewKey = targetViewKey;
        TargetType = targetType;
        TargetId = targetId;
    }
    
    
}