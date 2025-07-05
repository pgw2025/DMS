using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("VariableDataMqtt")]
public class DbVariableDataMqtt
{
    [SugarColumn(IsPrimaryKey = true)]
    public int VariableDataId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public int MqttId { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(VariableDataId))]
    public DbVariableData VariableData { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(MqttId))]
    public DbMqtt Mqtt { get; set; }
}