using PMSWPF.Enums;

namespace PMSWPF.Models;

public class VariableData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NodeId { get; set; }
    public ProtocolType ProtocolType { get; set; }
    public string DataType { get; set; }
    public List<Mqtt> Mqtts { get; set; }
    public string DataValue { get; set; }
    public string DisplayValue { get; set; }
    public DateTime UpdateTime { get; set; }
    public User UpdateUser { get; set; }
    public string Converstion { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public bool IsSave { get; set; }
    public double SaveRange { get; set; }
    public bool IsAlarm { get; set; }
    public double AlarmMin { get; set; }
    public double AlarmMax { get; set; }
    public SignalType SignalType { get; set; }
}