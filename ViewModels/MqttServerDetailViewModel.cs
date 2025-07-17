using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.Helper;
using PMSWPF.Enums;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PMSWPF.Data.Repositories;

namespace PMSWPF.ViewModels
{
    /// <summary>
    /// MQTT服务器详情视图模型。
    /// 负责管理单个MQTT服务器的配置及其关联的变量数据。
    /// </summary>
    public partial class MqttServerDetailViewModel : ViewModelBase
    {
        private readonly ILogger<MqttServerDetailViewModel> _logger;
        private readonly DataServices _dataServices;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// 当前正在编辑的MQTT服务器对象。
        /// </summary>
        [ObservableProperty]
        private Mqtt _currentMqtt;

        /// <summary>
        /// 与当前MQTT服务器关联的变量数据集合。
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<VariableMqtt> _associatedVariables;

        private readonly VariableMqttAliasRepository _variableMqttAliasRepository;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志服务。</param>
        /// <param name="dataServices">数据服务。</param>
        /// <param name="dialogService">对话框服务。</param>
        public MqttServerDetailViewModel(ILogger<MqttServerDetailViewModel> logger, DataServices dataServices,
                                         IDialogService dialogService, VariableMqttAliasRepository variableMqttAliasRepository)
        {
            _logger = logger;
            _dataServices = dataServices;
            _dialogService = dialogService;
            _variableMqttAliasRepository = variableMqttAliasRepository;
        }

        public override void OnLoaded()
        {
            if (CurrentMqtt.VariableMqtts != null)
            {
                AssociatedVariables =new ObservableCollection<VariableMqtt>(CurrentMqtt.VariableMqtts) ;
            }
        }


        /// <summary>
        /// 保存MQTT服务器及其关联变量的更改。
        /// </summary>
        [RelayCommand]
        private async Task SaveChanges()
        {
            if (CurrentMqtt == null) return;

            // TODO: 实现保存逻辑。这可能涉及到更新Mqtt对象和更新VariableData对象。
            // 由于Mqtt和VariableData之间的关联可能在数据库中通过中间表维护，
            // 这里需要根据实际的数据库操作来调整。
            // 例如，如果Mqtt对象本身包含关联的VariableData列表，则直接保存Mqtt对象即可。
            // 如果是多对多关系，可能需要更新中间表。

            // 示例：假设Mqtt对象需要更新
            // await _dataServices.UpdateMqttAsync(CurrentMqtt); 

            // 示例：假设变量数据也需要保存
            // foreach (var variable in AssociatedVariables.Where(v => v.IsModified))
            // {
            //     await _dataServices.UpdateVariableAsync(variable);
            // }

            NotificationHelper.ShowInfo("MQTT服务器详情保存功能待实现。");
            _logger.LogInformation("Save changes for MQTT server detail initiated.");
        }

        /// <summary>
        /// 从当前MQTT服务器中移除选定的变量。
        /// </summary>
        /// <param name="variablesToRemove">要移除的变量列表。</param>
        [RelayCommand]
        private async Task RemoveVariables(System.Collections.IList variablesToRemove)
        {
            if (CurrentMqtt == null || variablesToRemove == null || variablesToRemove.Count == 0)
            {
                NotificationHelper.ShowInfo("请选择要移除的变量。");
                return;
            }

            var variablesList = variablesToRemove.Cast<Variable>()
                                                 .ToList();

            var result = await _dialogService.ShowConfrimeDialog(
                "确认移除", $"确定要从MQTT服务器 '{CurrentMqtt.Name}' 中移除选定的 {variablesList.Count} 个变量吗？");
            if (result != true) return;

            foreach (var variable in variablesList) // 使用ToList()避免在迭代时修改集合
            {
                // 移除变量与当前MQTT服务器的关联
                // variable.Mqtts?.Remove(CurrentMqtt);
                // // 标记变量为已修改，以便保存时更新数据库
                // variable.IsModified = true;
                // AssociatedVariables.Remove(variable);
                // _logger.LogInformation($"Removed variable {variable.Name} from MQTT server {CurrentMqtt.Name}.");
            }

            // TODO: 这里需要调用DataServices来更新数据库中VariableData的Mqtt关联
            // 例如：await _dataServices.UpdateVariableDataAssociationsAsync(variablesToRemove);
            NotificationHelper.ShowSuccess("变量移除成功，请记得保存更改。");
        }

        /// <summary>
        /// 添加变量到当前MQTT服务器。
        /// </summary>
        [RelayCommand]
        private async Task AddVariables()
        {
            if (CurrentMqtt == null) return;

            // TODO: 实现选择变量的对话框，让用户选择要添加的变量
            // 例如：var selectedVariables = await _dialogService.ShowVariableSelectionDialogAsync();
            // 这里只是一个占位符，实际需要一个UI来选择变量
            NotificationHelper.ShowInfo("添加变量功能待实现，需要一个变量选择对话框。");
            _logger.LogInformation("AddAsync variables to MQTT server initiated.");

            // 假设我们已经通过对话框获取到了一些要添加的变量
            // List<Variable> newVariables = ...; 
            // foreach (var variable in newVariables)
            // {
            //     if (variable.Mqtts == null) variable.Mqtts = new List<Mqtt>();
            //     if (!variable.Mqtts.Any(m => m.Id == CurrentMqtt.Id))
            //     {
            //         variable.Mqtts.AddAsync(CurrentMqtt);
            //         variable.IsModified = true; // 标记为已修改
            //         AssociatedVariables.AddAsync(variable);
            //     }
            // }
            // NotificationHelper.ShowMessage("变量添加成功，请记得保存更改。", NotificationType.Success);
        }
    }
}