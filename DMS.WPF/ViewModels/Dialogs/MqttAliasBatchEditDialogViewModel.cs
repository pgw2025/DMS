using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.WPF.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// MQTT别名批量编辑对话框的视图模型
    /// </summary>
    public partial class MqttAliasBatchEditDialogViewModel : DialogViewModelBase<List<VariableMqttAliasItemViewModel>>
    {
        [ObservableProperty]
        private ObservableCollection<VariableMqttAliasItemViewModel> _variableMqttAliases = new();

        [ObservableProperty]
        private MqttServerItemViewModel _selectedMqttServer;

        public MqttAliasBatchEditDialogViewModel(
            List<VariableItemViewModel> variables, 
            MqttServerItemViewModel mqttServer)
        {
            _selectedMqttServer = mqttServer;
            InitializeVariableMqttAliases(variables);
        }

        /// <summary>
        /// 初始化变量MQTT别名列表
        /// </summary>
        private void InitializeVariableMqttAliases(List<VariableItemViewModel> variables)
        {
            VariableMqttAliases.Clear();

            foreach (var variable in variables)
            {
                // 检查该变量是否已经有针对此MQTT服务器的别名
                var existingAlias = variable.MqttAliases?.FirstOrDefault(ma => ma.MqttServerId == SelectedMqttServer.Id);

                var variableMqttAlias = new VariableMqttAliasItemViewModel
                {
                    VariableId = variable.Id,
                    MqttServerId = SelectedMqttServer.Id,
                    MqttServerName = SelectedMqttServer.ServerName,
                    MqttServer = SelectedMqttServer,
                    Variable = variable,
                    Alias = existingAlias?.Alias ?? GenerateDefaultAlias(variable)
                };

                VariableMqttAliases.Add(variableMqttAlias);
            }
        }

        /// <summary>
        /// 生成默认别名
        /// </summary>
        private string GenerateDefaultAlias(VariableItemViewModel variable)
        {
            // 可以根据需要自定义默认别名生成逻辑
            return $"{variable.Name}";
        }

        /// <summary>
        /// 确认编辑
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            var result = VariableMqttAliases.ToList();
            Close(result);
        }

        /// <summary>
        /// 取消编辑
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Close(null);
        }

        /// <summary>
        /// 全部应用相同的别名前缀
        /// </summary>
        [RelayCommand]
        private void ApplySamePrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return;

            foreach (var alias in VariableMqttAliases)
            {
                alias.Alias = $"{prefix}_{alias.Variable.Name}";
            }
        }
    }
}