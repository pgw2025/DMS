using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Core.Models.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ItemViewModel;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace DMS.WPF.ViewModels
{
    /// <summary>
    /// 触发器管理视图模型
    /// </summary>
    public partial class TriggersViewModel : ViewModelBase
    {
        private readonly IMapper _mapper;
        private readonly ITriggerDataService _triggerDataService;
        private readonly IDataStorageService _dataStorageService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableDictionary<int, TriggerItem> _triggers ;

        [ObservableProperty]
        private TriggerItem? _selectedTrigger;

        public TriggersViewModel(
            IMapper mapper,
            ITriggerDataService triggerDataService,
            IDataStorageService dataStorageService,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _triggerDataService = triggerDataService ?? throw new ArgumentNullException(nameof(triggerDataService));
            _dataStorageService = dataStorageService ?? throw new ArgumentNullException(nameof(dataStorageService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            
            // 初始化时加载触发器数据
            Triggers=_dataStorageService.Triggers;
        }

  

        /// <summary>
        /// 添加新触发器
        /// </summary>
        [RelayCommand]
        private async Task AddTriggerAsync()
        {
            var newTrigger = new TriggerItem()
                {
                    IsActive = true,
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
                    // 使用TriggerDataService添加触发器
                    var createdTrigger = await _triggerDataService.AddTrigger(newTrigger);
                    
                    if (createdTrigger != null )
                    {
                        // 触发器已添加到数据存储中，只需更新本地集合
                        _notificationService.ShowSuccess("触发器创建成功");
                    }
                    else
                    {
                        _notificationService.ShowError("触发器创建失败");
                    }
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
            var triggerToEdit = _mapper.Map<Trigger>(SelectedTrigger);
            
            TriggerDialogViewModel viewModel = App.Current.Services.GetRequiredService<TriggerDialogViewModel>();
            await viewModel.OnInitializedAsync(triggerToEdit);

            var result = await _dialogService.ShowDialogAsync(viewModel);
            if (result != null)
            {
                try
                {
                    // 使用TriggerDataService更新触发器
                    var updatedTrigger = await _triggerDataService.UpdateTrigger(SelectedTrigger);
                    if (updatedTrigger)
                    {
                        _notificationService.ShowSuccess("触发器更新成功");
                    }
                    else
                    {
                        _notificationService.ShowError("触发器更新失败");
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
                    // 使用TriggerDataService删除触发器
                    var success = await _triggerDataService.DeleteTrigger(SelectedTrigger);
                    if (success)
                    {
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

    }
}