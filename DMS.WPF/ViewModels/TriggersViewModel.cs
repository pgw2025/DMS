using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.Core.Models;
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
        private readonly IWpfDataService _dataStorageService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly INavigationService _navigationService;

        public ISynchronizedView<KeyValuePair<int, TriggerItem>, TriggerItem> _synchronizedView;
        public NotifyCollectionChangedSynchronizedViewList<TriggerItem> TriggerItemListView { get; }

        [ObservableProperty]
        private ObservableDictionary<int, TriggerItem> _triggers;

        [ObservableProperty]
        private TriggerItem? _selectedTrigger;

        public TriggersViewModel(
            IMapper mapper,
            ITriggerDataService triggerDataService,
            IWpfDataService dataStorageService,
            IDialogService dialogService,
            INotificationService notificationService,
            INavigationService navigationService)
        {
            _mapper = mapper;
            _triggerDataService = triggerDataService ?? throw new ArgumentNullException(nameof(triggerDataService));
            _dataStorageService = dataStorageService ?? throw new ArgumentNullException(nameof(dataStorageService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // 初始化时加载触发器数据
            _synchronizedView = _dataStorageService.Triggers.CreateView(v => v.Value);
            TriggerItemListView = _synchronizedView.ToNotifyCollectionChanged();



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
                Name = "新建触发器",
                Description = "新建触发器",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            TriggerDialogViewModel viewModel = App.Current.Services.GetRequiredService<TriggerDialogViewModel>();
            await viewModel.OnInitializedAsync(newTrigger);

            var result = await _dialogService.ShowDialogAsync(viewModel);
            if (result is null) return;

            try
            {

                // 使用TriggerDataService添加触发器和菜单
                var resTriggerItem = await _triggerDataService.AddTrigger(result);

                if (resTriggerItem is not null)
                {
                    // 更新UI显示
                    _notificationService.ShowSuccess($"触发器创建成功：{resTriggerItem.Name}");
                }
                else
                {
                    _notificationService.ShowError("触发器创建失败");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"创建触发器失败: {ex.Message},");
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
            TriggerItem triggerItemToEdit = new TriggerItem();
             _mapper.Map(SelectedTrigger, triggerItemToEdit);

            TriggerDialogViewModel viewModel = App.Current.Services.GetRequiredService<TriggerDialogViewModel>();
            await viewModel.OnInitializedAsync(triggerItemToEdit);

            var result = await _dialogService.ShowDialogAsync(viewModel);
            if (result != null)
            {
                try
                {
                    // 使用TriggerDataService更新触发器
                    var updatedTrigger = await _triggerDataService.UpdateTrigger(result);
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

            var confirm = await _dialogService.ShowDialogAsync(new ConfirmDialogViewModel("确认删除", $"确定要删除触发器 '{SelectedTrigger.Description}' 吗？", "删除"));
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

        /// <summary>
        /// 刷新触发器列表
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                // 重新加载所有触发器数据
                _triggerDataService.LoadAllTriggers();
                _notificationService.ShowSuccess("触发器列表已刷新");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"刷新触发器列表失败: {ex.Message}");
            }
        }

        [RelayCommand]
        public void NavigateToTriggerDetail()
        {
            if (SelectedTrigger == null) return;

            _navigationService.NavigateToAsync(this, new NavigationParameter(nameof(TriggerDetailViewModel), SelectedTrigger.Id, NavigationType.Trigger));
        }

    }
}