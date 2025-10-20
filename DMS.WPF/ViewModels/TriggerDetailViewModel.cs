using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Core.Models.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using DMS.WPF.ViewModels.Dialogs;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.ObjectModel;
using DMS.Core.Models;

namespace DMS.WPF.ViewModels
{
    /// <summary>
    /// 触发器详情视图模型。
    /// 负责管理单个触发器的配置及其关联的变量数据。
    /// </summary>
    public partial class TriggerDetailViewModel : ViewModelBase
    {
        private readonly ILogger<TriggerDetailViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly ITriggerManagementService _triggerManagementService;
        private readonly ITriggerDataService _triggerDataService;
        private readonly IWpfDataService _dataStorageService;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// 当前正在查看的触发器对象。
        /// </summary>
        [ObservableProperty]
        private TriggerItem _currentTrigger;


        [ObservableProperty]
        private IList _selectedVariables = new ArrayList();

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="logger">日志服务。</param>
        /// <param name="dialogService">对话框服务。</param>
        /// <param name="notificationService">通知服务。</param>
        /// <param name="triggerManagementService">触发器管理服务</param>
        /// <param name="triggerDataService">触发器数据服务</param>
        /// <param name="dataStorageService">数据存储服务</param>
        /// <param name="navigationService">导航服务</param>
        public TriggerDetailViewModel(ILogger<TriggerDetailViewModel> logger,
                                      IDialogService dialogService,
                                      INotificationService notificationService,
                                      ITriggerManagementService triggerManagementService,
                                      ITriggerDataService triggerDataService,
                                      IWpfDataService dataStorageService,
                                      INavigationService navigationService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _notificationService = notificationService;
            _triggerManagementService = triggerManagementService;
            _triggerDataService = triggerDataService;
            _dataStorageService = dataStorageService;
            _navigationService = navigationService;
        }

        /// <summary>
        /// 编辑当前触发器
        /// </summary>
        [RelayCommand]
        private async Task EditTrigger()
        {
            try
            {
                if (CurrentTrigger == null)
                {
                    _notificationService.ShowError("没有选中的触发器，无法编辑。");
                    return;
                }

                // 创建编辑对话框的视图模型
                TriggerDialogViewModel triggerDialogViewModel = new TriggerDialogViewModel(_dialogService, _dataStorageService, _notificationService);
                await triggerDialogViewModel.OnInitializedAsync(CurrentTrigger);

                // 显示对话框
                var updatedTrigger = await _dialogService.ShowDialogAsync(triggerDialogViewModel);

                if (updatedTrigger == null)
                {
                    return; // 用户取消了编辑
                }

                // 更新触发器
                var result = await _triggerDataService.UpdateTrigger(updatedTrigger);

                if (result)
                {
                    // 更新当前视图模型的数据
                    CurrentTrigger.Name = updatedTrigger.Name;
                    CurrentTrigger.Description = updatedTrigger.Description;
                    CurrentTrigger.IsActive = updatedTrigger.IsActive;
                    CurrentTrigger.Action = updatedTrigger.Action;
                    CurrentTrigger.ActionConfigurationJson = updatedTrigger.ActionConfigurationJson;
                    CurrentTrigger.SuppressionDuration = updatedTrigger.SuppressionDuration;
                    CurrentTrigger.UpdatedAt = updatedTrigger.UpdatedAt;

                    _notificationService.ShowSuccess($"触发器编辑成功：{updatedTrigger.Name}");
                }
                else
                {
                    _notificationService.ShowError("更新触发器失败。");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "编辑触发器过程中发生错误");
                _notificationService.ShowError($"编辑触发器过程中发生错误：{e.Message}", e);
            }
        }

        /// <summary>
        /// 重新加载当前触发器数据
        /// </summary>
        [RelayCommand]
        private async Task Reload()
        {
            if (CurrentTrigger?.Id > 0)
            {
                // 重新加载当前触发器数据
                var updatedTrigger = await _triggerManagementService.GetTriggerByIdAsync(CurrentTrigger.Id);
                if (updatedTrigger != null)
                {
                    // 更新CurrentTrigger的属性
                    CurrentTrigger.Name = updatedTrigger.Name;
                    CurrentTrigger.Description = updatedTrigger.Description;
                    CurrentTrigger.IsActive = updatedTrigger.IsActive;
                    CurrentTrigger.Action = updatedTrigger.Action;
                    CurrentTrigger.ActionConfigurationJson = updatedTrigger.ActionConfigurationJson;
                    CurrentTrigger.SuppressionDuration = updatedTrigger.SuppressionDuration;
                    CurrentTrigger.LastTriggeredAt = updatedTrigger.LastTriggeredAt;
                    CurrentTrigger.UpdatedAt = updatedTrigger.UpdatedAt;
                    CurrentTrigger.CreatedAt = updatedTrigger.CreatedAt;
                }
            }
        }

        /// <summary>
        /// 导航回触发器列表页面
        /// </summary>
        [RelayCommand]
        private async Task NavigateToTriggers()
        {
            await _navigationService.NavigateToAsync(this, new NavigationParameter(nameof(TriggersViewModel)));
        }

        public override Task OnNavigatedToAsync(NavigationParameter parameter)
        {
            if (parameter == null) return Task.CompletedTask;

            if (_dataStorageService.Triggers.TryGetValue(parameter.TargetId, out var triggerItem))
            {
                CurrentTrigger = triggerItem;

            }

            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(NavigationParameter parameter)
        {
            return Task.CompletedTask;
        }
    }
}