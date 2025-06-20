using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Models;

public partial class Device:ObservableObject
{
    [ObservableProperty]
    private int id;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string description ;
    [ObservableProperty]
    private string ip ;
    [ObservableProperty]
    private bool isActive =true;
    [ObservableProperty]
    private bool isRuning ;
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    public List<DataVariable>? DataVariables { get; set; } 
    public ProtocolType ProtocolType { get; set; }
    
}