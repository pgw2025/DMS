using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Models;

public partial class Device : ObservableObject
{
    [ObservableProperty] private string description;

    [ObservableProperty] private int id;

    [ObservableProperty] private string ip;

    [ObservableProperty] private bool isActive = true;

    [ObservableProperty] private bool isRuning;

    [ObservableProperty] private string name;

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    public List<VariableTable>? VariableTables { get; set; }
    public ProtocolType ProtocolType { get; set; }
}