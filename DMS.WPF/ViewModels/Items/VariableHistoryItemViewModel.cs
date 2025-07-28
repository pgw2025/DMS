using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using System;

namespace DMS.WPF.ViewModels.Items;

public partial class VariableHistoryItemViewModel : ObservableObject
{
    public long Id { get; }

    [ObservableProperty]
    private int _variableId;

    [ObservableProperty]
    private string _value;

    [ObservableProperty]
    private DateTime _timestamp;

    
}
