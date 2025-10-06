using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using System;

namespace DMS.WPF.ItemViewModel;

public partial class VariableHistoryItem : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty]
    private int _variableId;

    [ObservableProperty]
    private string _value;

    [ObservableProperty]
    private DateTime _timestamp;

    
}
