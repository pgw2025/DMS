using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using System;
using System.Collections.Generic;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 表示单个变量项的视图模型。
/// 此类用于UI层的数据绑定，封装了变量数据以及与UI相关的状态和逻辑。
/// 它继承自 ObservableObject，以便于实现 INotifyPropertyChanged 接口，从而支持WPF的双向绑定。
/// </summary>
public partial class VariableItemViewModel : ObservableObject
{
    /// <summary>
    /// 获取或设置变量的唯一标识符 (ID)。
    /// 通常对应数据库中的主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置变量的名称。
    /// 这个名称会显示在UI界面上，例如在列表或标签中。
    /// [ObservableProperty] 特性会自动生成支持变更通知的属性。
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// 获取或设置变量的S7通信地址。
    /// 例如："DB1.DBD0"。仅在协议为S7时有效。
    /// </summary>
    [ObservableProperty]
    private string? _s7Address;

    /// <summary>
    /// 获取或设置从设备读取到的原始数据值。
    /// 这通常是未经转换的字符串形式的值。
    /// </summary>
    [ObservableProperty]
    private string? _dataValue;

    /// <summary>
    /// 获取或设置用于在界面上显示的值。
    /// 这可能是经过公式转换、格式化或添加了单位后的值。
    /// </summary>
    [ObservableProperty]
    private string? _displayValue;

    /// <summary>
    /// 获取或设置此变量所属的变量表 (VariableTable) 的数据传输对象 (DTO)。
    /// 用于在界面上显示变量表的关联信息。
    /// </summary>
    [ObservableProperty]
    private VariableTableDto? _variableTable;

    /// <summary>
    /// 获取或设置与此变量关联的MQTT别名列表。
    /// 一个变量可以有多个MQTT别名。
    /// </summary>
    [ObservableProperty]
    private List<VariableMqttAliasDto>? _mqttAliases;

    /// <summary>
    /// 获取或设置变量的信号类型 (如：AI, DI, AO, DO)。
    /// </summary>
    [ObservableProperty]
    private SignalType _signalType =SignalType.OtherASignal;

    /// <summary>
    /// 获取或设置变量的轮询等级。
    /// 用于决定数据采集的频率（如：高、中、低）。
    /// </summary>
    [ObservableProperty]
    private PollLevelType _pollLevel=PollLevelType.ThirtySeconds;

    /// <summary>
    /// 获取或设置一个值，该值指示此变量是否被激活。
    /// 未激活的变量将不会被后台服务轮询。
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    /// <summary>
    /// 获取或设置变量所属的变量表的ID。
    /// </summary>
    [ObservableProperty]
    private int _variableTableId;

    /// <summary>
    /// 获取或设置变量的OPC UA节点ID。
    /// 例如："ns=2;s=MyDevice.MyVariable"。仅在协议为OpcUa时有效。
    /// </summary>
    [ObservableProperty]
    private string _opcUaNodeId;

    /// <summary>
    /// 获取或设置是否为此变量启用历史记录。
    /// </summary>
    [ObservableProperty]
    private bool _isHistoryEnabled;

    /// <summary>
    /// 获取或设置历史记录的死区值。
    /// 当值的变化超过此死区值时，才会记录历史数据，用于减少不必要的数据存储。
    /// </summary>
    [ObservableProperty]
    private double _historyDeadband;

    /// <summary>
    /// 获取或设置是否为此变量启用报警功能。
    /// </summary>
    [ObservableProperty]
    private bool _isAlarmEnabled;

    /// <summary>
    /// 获取或设置报警的下限值。
    /// </summary>
    [ObservableProperty]
    private double _alarmMinValue;

    /// <summary>
    /// 获取或设置报警的上限值。
    /// </summary>
    [ObservableProperty]
    private double _alarmMaxValue;

    /// <summary>
    /// 获取或设置报警的死区值。
    /// 用于防止在临界点附近频繁触发或解除报警。
    /// </summary>
    [ObservableProperty]
    private double _alarmDeadband;

    /// <summary>
    /// 获取或设置变量使用的通信协议（如：S7, OpcUa, Modbus）。
    /// </summary>
    [ObservableProperty]
    private ProtocolType _protocol;

    /// <summary>
    /// 获取或设置变量在C#中对应的数据类型。
    /// </summary>
    [ObservableProperty]
    private DataType _dataType;

    /// <summary>
    /// 获取或设置值的转换公式。
    /// 例如："x * 10" 或 "(x / 100) + 5"。'x' 代表原始值。
    /// </summary>
    [ObservableProperty]
    private string _conversionFormula;

    /// <summary>
    /// 获取或设置记录的创建时间。
    /// </summary>
    [ObservableProperty]
    private DateTime _createdAt;

    /// <summary>
    /// 获取或设置记录的最后更新时间。
    /// </summary>
    [ObservableProperty]
    private DateTime _updatedAt;

    /// <summary>
    /// 获取或设置最后更新此记录的用户或进程。
    /// </summary>
    [ObservableProperty]
    private string _updatedBy;

    /// <summary>
    /// 获取或设置一个值，该值指示此视图模型中的数据是否已被修改。
    /// 用于跟踪UI上的更改，以便提示用户保存。
    /// </summary>
    [ObservableProperty]
    private bool _isModified;

    /// <summary>
    /// 获取或设置变量的描述信息。
    /// 提供关于该变量的更多上下文或备注。
    /// </summary>
    [ObservableProperty]
    private string _description;


    /// <summary>
    /// 获取或设置OPC UA值的更新方式（轮询或订阅）。
    /// </summary>
    [ObservableProperty]
    private OpcUaUpdateType _opcUaUpdateType;

    
}