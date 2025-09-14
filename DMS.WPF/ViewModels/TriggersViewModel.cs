using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs.Triggers;
using DMS.Application.Services.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.ViewModels.Triggers
{
    /// <summary>
    /// 触发器管理视图模型
    /// </summary>
    public partial class TriggersViewModel : ViewModelBase
    {
        private readonly ITriggerManagementService _triggerManagementService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<TriggerDefinitionDto> _triggers = new();

        [ObservableProperty]
        private TriggerDefinitionDto? _selectedTrigger;

        public TriggersViewModel(
            ITriggerManagementService triggerManagementService,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _triggerManagementService = triggerManagementService ?? throw new ArgumentNullException(nameof(triggerManagementService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// 加载所有触发器
        /// </summary>
        [RelayCommand]
        private async Task LoadTriggersAsync()
        {
            try
            {
                var triggerList = await _triggerManagementService.GetAllTriggersAsync();
                Triggers = new ObservableCollection<TriggerDefinitionDto>(triggerList);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"加载触发器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加新触发器
        /// </summary>
        [RelayCommand]
        private async Task AddTriggerAsync()
        {
            var newTrigger = new TriggerDefinitionDto
            {
                IsActive = true,
                Condition = Core.Models.Triggers.ConditionType.GreaterThan,
                Action = Core.Models.Triggers.ActionType.SendEmail,
                Description = "新建触发器",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            TriggerDialogViewModel viewModel = App.Current.Services.GetRequiredService<TriggerDialogViewModel>();
            await viewModel.OnInitializedAsync(newTrigger);

            var result = await _dialogService.ShowDialogAsync(viewModel);
            if (result != null)
            {
                try
                {
                    var createdTrigger = await _triggerManagementService.CreateTriggerAsync(result);
                    Triggers.Add(createdTrigger);
                    SelectedTrigger = createdTrigger;
                    _notificationService.ShowSuccess("触发器创建成功");
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError($"创建触发器失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 编辑选中的触发器
        /// </summary>
        [RelayCommand]
        private async Task EditTriggerAsync()
        {
            if (SelectedTrigger == null)
            {
                _notificationService.ShowWarn("请先选择一个触发器");
                return;
            }

            // 传递副本以避免直接修改原始对象
            var triggerToEdit = new TriggerDefinitionDto
            {
                Id = SelectedTrigger.Id,
                VariableId = SelectedTrigger.VariableId,
                IsActive = SelectedTrigger.IsActive,
                Condition = SelectedTrigger.Condition,
                Threshold = SelectedTrigger.Threshold,
                LowerBound = SelectedTrigger.LowerBound,
                UpperBound = SelectedTrigger.UpperBound,
                Action = SelectedTrigger.Action,
                ActionConfigurationJson = SelectedTrigger.ActionConfigurationJson,
                SuppressionDuration = SelectedTrigger.SuppressionDuration,
                LastTriggeredAt = SelectedTrigger.LastTriggeredAt,
                Description = SelectedTrigger.Description,
                CreatedAt = SelectedTrigger.CreatedAt,
                UpdatedAt = SelectedTrigger.UpdatedAt
            };
            TriggerDialogViewModel viewModel = App.Current.Services.GetRequiredService<TriggerDialogViewModel>();
            await viewModel.OnInitializedAsync(triggerToEdit);

            var result = await _dialogService.ShowDialogAsync(viewModel);
            if (result != null)
            {
                try
                {
                    var updatedTrigger = await _triggerManagementService.UpdateTriggerAsync(result.Id, result);
                    if (updatedTrigger != null)
                    {
                        var index = Triggers.IndexOf(SelectedTrigger);
                        if (index >= 0)
                        {
                            Triggers[index] = updatedTrigger;
                        }
                        SelectedTrigger = updatedTrigger;
                        _notificationService.ShowSuccess("触发器更新成功");
                    }
                    else
                    {
                        _notificationService.ShowError("触发器更新失败，未找到对应记录");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError($"更新触发器失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 删除选中的触发器
        /// </summary>
        [RelayCommand]
        private async Task DeleteTriggerAsync()
        {
            if (SelectedTrigger == null)
            {
                _notificationService.ShowWarn("请先选择一个触发器");
                return;
            }

            var confirm = await _dialogService.ShowDialogAsync(new ConfirmDialogViewModel("确认删除", $"确定要删除触发器 '{SelectedTrigger.Description}' 吗？","删除"));
            if (confirm)
            {
                try
                {
                    var success = await _triggerManagementService.DeleteTriggerAsync(SelectedTrigger.Id);
                    if (success)
                    {
                        Triggers.Remove(SelectedTrigger);
                        SelectedTrigger = Triggers.FirstOrDefault();
                        _notificationService.ShowSuccess("触发器删除成功");
                    }
                    else
                    {
                        _notificationService.ShowError("触发器删除失败");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError($"删除触发器失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 视图加载时执行的命令
        /// </summary>
        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            await LoadTriggersCommand.ExecuteAsync(null);
        }
    }
}