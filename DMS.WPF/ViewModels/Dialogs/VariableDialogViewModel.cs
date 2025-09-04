using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VariableDialogViewModel : DialogViewModelBase<VariableItemViewModel>
{
    [ObservableProperty]
    private VariableItemViewModel _variable;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAddModel = true;

    [ObservableProperty]
    private bool _hasError;
    private readonly DataServices _dataServices;
    private readonly IVariableAppService _variableAppService;
    private readonly IMapper _mapper;

    public VariableDialogViewModel(DataServices dataServices, IVariableAppService variableAppService, IMapper mapper)
    {

        Variable = new VariableItemViewModel();

        this._dataServices = dataServices;
        this._variableAppService = variableAppService;
        this._mapper = mapper;
    }

    public async Task<bool> PrimaryButtonAsync()
    {
        // 清除之前的错误信息
        ErrorMessage = string.Empty;
        HasError = false;
        // 验证输入
        bool isValid = ValidateInputAsync();
        if (isValid)
        {
            Close(Variable);
        }

        return isValid;
    }

    private bool ValidateInputAsync()
    {
        // 检查变量名称是否为空
        if (string.IsNullOrWhiteSpace(Variable.Name))
        {
            ErrorMessage = "变量名称不能为空";
            HasError = true;
            return false;
        }

        // 根据协议类型检查地址
        if (Variable.Protocol == Core.Enums.ProtocolType.S7 &&
            string.IsNullOrWhiteSpace(Variable.S7Address))
        {
            ErrorMessage = "S7协议必须填写S7地址";
            HasError = true;
            return false;
        }

        if (Variable.Protocol == Core.Enums.ProtocolType.OpcUa &&
            string.IsNullOrWhiteSpace(Variable.OpcUaNodeId))
        {
            ErrorMessage = "OPC UA协议必须填写节点ID";
            HasError = true;
            return false;
        }
        //检查变量是否存在
        var existVariables = _dataServices.Variables.Where(v => v.Name == Variable.Name || (v.Protocol == ProtocolType.S7 && v.S7Address == Variable.S7Address) || (v.Protocol == ProtocolType.OpcUa && v.OpcUaNodeId == Variable.OpcUaNodeId)).ToList();
        VariableItemViewModel existVariable = null;
        if (IsAddModel)
        {
            existVariable = existVariables.FirstOrDefault();
        }
        else
        {
            existVariable = existVariables.FirstOrDefault(v => v.Id != Variable.Id);
        }

        if (existVariable != null)
        {

            if (Variable.Name == existVariable.Name)
            {
                ErrorMessage = $"变量名称:{existVariable.Name}已经存在。";
                HasError = true;
                return false;
            }

            if (Variable.Protocol == Core.Enums.ProtocolType.S7 && Variable.S7Address == existVariable.S7Address)
            {
                ErrorMessage = $"变量S7地址:{existVariable.S7Address}已经存在。";
                HasError = true;
                return false;
            }

            if (Variable.Protocol == Core.Enums.ProtocolType.OpcUa && Variable.OpcUaNodeId == existVariable.OpcUaNodeId)
            {
                ErrorMessage = $"变量OpcUa节点ID:{existVariable.OpcUaNodeId}已经存在。";
                HasError = true;
                return false;
            }

        }

        return true;
    }

    [RelayCommand]
    private void CancleButton()
    {
        Close(null);
    }
}