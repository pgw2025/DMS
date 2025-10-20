using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Models.Triggers;
using DMS.WPF.ItemViewModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// 触发器选择对话框的视图模型
    /// </summary>
    public partial class TriggerSelectionDialogViewModel : DialogViewModelBase<TriggerItem>
    {
        private readonly ITriggerAppService _triggerAppService;

        [ObservableProperty]
        private ObservableCollection<TriggerItem> _triggers = new();

        [ObservableProperty]
        private TriggerItem _selectedTrigger;

        public TriggerSelectionDialogViewModel(ITriggerAppService triggerAppService)
        {
            _triggerAppService = triggerAppService;
            LoadTriggersAsync();
        }

        /// <summary>
        /// 异步加载所有触发器
        /// </summary>
        private async void LoadTriggersAsync()
        {
            try
            {
                var triggers = await _triggerAppService.GetAllTriggersAsync();
                Triggers.Clear();

                foreach (var trigger in triggers)
                {
                    Triggers.Add(new TriggerItem
                    {
                        Id = trigger.Id,
                        Name = trigger.Name,
                        Description = trigger.Description,
                        IsActive = trigger.IsActive,
                        Action = trigger.Action,
                        ActionConfigurationJson = trigger.ActionConfigurationJson,
                        SuppressionDuration = trigger.SuppressionDuration,
                        LastTriggeredAt = trigger.LastTriggeredAt,
                        CreatedAt = trigger.CreatedAt,
                        UpdatedAt = trigger.UpdatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志
                System.Console.WriteLine($"加载触发器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 确认选择
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            if (SelectedTrigger != null)
            {
                Close(SelectedTrigger);
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Close(null);
        }
    }
}