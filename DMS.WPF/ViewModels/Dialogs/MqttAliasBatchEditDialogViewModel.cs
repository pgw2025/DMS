using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;
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
    public partial class MqttAliasBatchEditDialogViewModel : DialogViewModelBase<List<MqttAliasItem>>
    {
        [ObservableProperty]
        private ObservableCollection<MqttAliasItem> _currentMqttAliases = new();

        [ObservableProperty]
        private MqttServerItem _selectedMqttServer;

        public MqttAliasBatchEditDialogViewModel(
            List<VariableItem> variables,
            MqttServerItem mqttServer)
        {
            _selectedMqttServer = mqttServer;
            InitializeCurrentMqttAliases(variables);
        }

        public MqttAliasBatchEditDialogViewModel(List<MqttAliasItem> mqttAliasItems)
        {
            foreach (var item in mqttAliasItems)
            {
                MqttAliasItem aliasItem = new MqttAliasItem();
                aliasItem.Id = item.Id;
                aliasItem.Alias = item.Alias;
                aliasItem.VariableId = item.VariableId;
                aliasItem.MqttServerId = item.MqttServerId;
                aliasItem.Variable = item.Variable;
                aliasItem.MqttServerName = item.MqttServerName;
                aliasItem.MqttServer = item.MqttServer;
                CurrentMqttAliases.Add(aliasItem);
            }
        }

        /// <summary>
        /// 初始化变量MQTT别名列表
        /// </summary>
        private void InitializeCurrentMqttAliases(List<VariableItem> variables)
        {
            CurrentMqttAliases.Clear();

            foreach (var variable in variables)
            {
                // 检查该变量是否已经有针对此MQTT服务器的别名
                var existingAlias = variable.MqttAliases?.FirstOrDefault(ma => ma.MqttServerId == SelectedMqttServer.Id);

                var variableMqttAlias = new MqttAliasItem
                {
                    VariableId = variable.Id,
                    MqttServerId = SelectedMqttServer.Id,
                    MqttServerName = SelectedMqttServer.ServerName,
                    MqttServer = SelectedMqttServer,
                    Variable = variable,
                    Alias = existingAlias?.Alias ?? GenerateDefaultAlias(variable)
                };

                CurrentMqttAliases.Add(variableMqttAlias);
            }
        }

        /// <summary>
        /// 生成默认别名
        /// </summary>
        private string GenerateDefaultAlias(VariableItem variable)
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
            var result = CurrentMqttAliases.ToList();
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

            foreach (var alias in CurrentMqttAliases)
            {
                alias.Alias = $"{prefix}_{alias.Variable.Name}";
            }
        }
    }
}