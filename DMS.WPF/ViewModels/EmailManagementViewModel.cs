using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Dialogs;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.ViewModels
{
    /// <summary>
    /// 邮件管理视图模型
    /// </summary>
    public partial class EmailManagementViewModel : ViewModelBase
    {
        private readonly IEmailAppService _emailAppService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<EmailAccountDto> _emailAccounts = new();

        [ObservableProperty]
        private EmailAccountDto? _selectedEmailAccount;

        [ObservableProperty]
        private ObservableCollection<EmailTemplateDto> _emailTemplates = new();

        [ObservableProperty]
        private EmailTemplateDto? _selectedEmailTemplate;

        public EmailManagementViewModel(
            IEmailAppService emailAppService,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _emailAppService = emailAppService;
            _dialogService = dialogService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// 加载数据命令
        /// </summary>
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                // 加载邮件账户
                var accounts = await _emailAppService.GetAllEmailAccountsAsync();
                EmailAccounts = new ObservableCollection<EmailAccountDto>(accounts);

                // 加载邮件模板
                var templates = await _emailAppService.GetAllEmailTemplatesAsync();
                EmailTemplates = new ObservableCollection<EmailTemplateDto>(templates);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("加载数据失败", ex);
            }
        }

        /// <summary>
        /// 添加邮件账户命令
        /// </summary>
        [RelayCommand]
        private async Task AddEmailAccountAsync()
        {
            EmailAccountDialogViewModel viewModel = App.Current.Services.GetRequiredService<EmailAccountDialogViewModel>();
            var emailAccountDto = await _dialogService.ShowDialogAsync(viewModel);
            if (emailAccountDto==null)
            {
                return;
            }
            
        }

        /// <summary>
        /// 编辑邮件账户命令
        /// </summary>
        [RelayCommand]
        private async Task EditEmailAccountAsync()
        {
            if (SelectedEmailAccount == null)
            {
                _notificationService.ShowWarn("请选择要编辑的邮件账户");
                return;
            }
            EmailAccountDialogViewModel viewModel = App.Current.Services.GetRequiredService<EmailAccountDialogViewModel>();
            viewModel.SetEmailAccount(SelectedEmailAccount);
            var emailAccountDto = await _dialogService.ShowDialogAsync(viewModel);
            if (emailAccountDto==null)
            {
                return;
            }

            // var dialog = _dialogService.CreateDialog<EmailAccountDialogViewModel>();
            // dialog.SetEmailAccount(SelectedEmailAccount);
            // var result = await dialog.ShowAsync();
            // if (result == true)
            // {
            //     await LoadDataAsync();
            // }
        }

        /// <summary>
        /// 删除邮件账户命令
        /// </summary>
        [RelayCommand]
        private async Task DeleteEmailAccountAsync()
        {
            if (SelectedEmailAccount == null)
            {
                _notificationService.ShowWarn("请选择要删除的邮件账户");
                return;
            }

            ConfirmDialogViewModel confirmDialogViewModel = new ConfirmDialogViewModel(
                "确认删除",
                $"确定要删除邮件账户 {SelectedEmailAccount.Name} 吗？", "删除");

            var confirmResult = await _dialogService.ShowDialogAsync(confirmDialogViewModel);

            if (confirmResult == true)
            {
                try
                {
                    await _emailAppService.DeleteEmailAccountAsync(SelectedEmailAccount.Id);
                    _notificationService.ShowSuccess("删除成功");
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("删除失败", ex);
                }
            }
        }

        /// <summary>
        /// 测试邮件账户连接命令
        /// </summary>
        [RelayCommand]
        private async Task TestEmailAccountAsync()
        {
            if (SelectedEmailAccount == null)
            {
                _notificationService.ShowWarn("请选择要测试的邮件账户");
                return;
            }

            try
            {
                var result = await _emailAppService.TestEmailAccountAsync(SelectedEmailAccount.Id);
                if (result)
                {
                    _notificationService.ShowSuccess("连接测试成功");
                }
                else
                {
                    _notificationService.ShowWarn("连接测试失败");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("连接测试失败", ex);
            }
        }

        /// <summary>
        /// 添加邮件模板命令
        /// </summary>
        [RelayCommand]
        private async Task AddEmailTemplateAsync()
        {
            // var dialog = _dialogService.CreateDialog<EmailTemplateDialogViewModel>();
            // var result = await dialog.ShowAsync();
            // if (result == true)
            // {
            //     await LoadDataAsync();
            // }
        }

        /// <summary>
        /// 编辑邮件模板命令
        /// </summary>
        [RelayCommand]
        private async Task EditEmailTemplateAsync()
        {
            if (SelectedEmailTemplate == null)
            {
                _notificationService.ShowWarn("请选择要编辑的邮件模板");
                return;
            }

            // var dialog = _dialogService.CreateDialog<EmailTemplateDialogViewModel>();
            // dialog.SetEmailTemplate(SelectedEmailTemplate);
            // var result = await dialog.ShowAsync();
            // if (result == true)
            // {
            //     await LoadDataAsync();
            // }
        }

        /// <summary>
        /// 删除邮件模板命令
        /// </summary>
        [RelayCommand]
        private async Task DeleteEmailTemplateAsync()
        {
            if (SelectedEmailTemplate == null)
            {
                _notificationService.ShowWarn("请选择要删除的邮件模板");
                return;
            }

            // var confirmResult = await _dialogService.ShowConfirmDialogAsync(
            //     "确认删除", 
            //     $"确定要删除邮件模板 {SelectedEmailTemplate.Name} 吗？");

            // if (confirmResult == true)
            // {
            //     try
            //     {
            //         await _emailAppService.DeleteEmailTemplateAsync(SelectedEmailTemplate.Id);
            //         _notificationService.ShowSuccess("删除成功");
            //         await LoadDataAsync();
            //     }
            //     catch (Exception ex)
            //     {
            //         _notificationService.ShowError("删除失败", ex);
            //     }
            // }
        }
    }
}