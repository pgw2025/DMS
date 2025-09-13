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
        private readonly IEmailDataService _emailDataService;
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
            IEmailDataService emailDataService,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _emailAppService = emailAppService;
            _emailDataService = emailDataService;
            _dialogService = dialogService;
            _notificationService = notificationService;
            
            // 绑定数据集合
            _emailAccounts = _emailDataService.EmailAccounts;
            _emailTemplates = _emailDataService.EmailTemplates;
        }

        /// <summary>
        /// 加载数据命令
        /// </summary>
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                // 使用EmailDataService加载所有邮件数据
                _emailDataService.LoadAllEmailData();
                
                // 等待一段时间确保数据加载完成
                await Task.Delay(100);
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
            
            try
            {
                var request = new CreateEmailAccountRequest
                {
                    Name = emailAccountDto.Name,
                    EmailAddress = emailAccountDto.EmailAddress,
                    SmtpServer = emailAccountDto.SmtpServer,
                    SmtpPort = emailAccountDto.SmtpPort,
                    EnableSsl = emailAccountDto.EnableSsl,
                    Username = emailAccountDto.Username,
                    Password = emailAccountDto.Password,
                    ImapServer = emailAccountDto.ImapServer,
                    ImapPort = emailAccountDto.ImapPort,
                    IsDefault = emailAccountDto.IsDefault,
                    IsActive = emailAccountDto.IsActive
                };
                
                await _emailDataService.AddEmailAccountAsync(request);
                _notificationService.ShowSuccess("添加邮件账户成功");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("添加邮件账户失败", ex);
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

            try
            {
                var request = new CreateEmailAccountRequest
                {
                    Name = emailAccountDto.Name,
                    EmailAddress = emailAccountDto.EmailAddress,
                    SmtpServer = emailAccountDto.SmtpServer,
                    SmtpPort = emailAccountDto.SmtpPort,
                    EnableSsl = emailAccountDto.EnableSsl,
                    Username = emailAccountDto.Username,
                    Password = emailAccountDto.Password,
                    ImapServer = emailAccountDto.ImapServer,
                    ImapPort = emailAccountDto.ImapPort,
                    IsDefault = emailAccountDto.IsDefault,
                    IsActive = emailAccountDto.IsActive
                };
                
                var result = await _emailDataService.UpdateEmailAccountAsync(SelectedEmailAccount.Id, request);
                if (result)
                {
                    _notificationService.ShowSuccess("更新邮件账户成功");
                }
                else
                {
                    _notificationService.ShowError("更新邮件账户失败");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("更新邮件账户失败", ex);
            }
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
                    var result = await _emailDataService.DeleteEmailAccountAsync(SelectedEmailAccount.Id);
                    if (result)
                    {
                        _notificationService.ShowSuccess("删除成功");
                    }
                    else
                    {
                        _notificationService.ShowError("删除失败");
                    }
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
                var result = await _emailDataService.TestEmailAccountAsync(SelectedEmailAccount.Id);
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
            var viewModel = App.Current.Services.GetRequiredService<EmailTemplateDialogViewModel>();
            var emailTemplateDto = await _dialogService.ShowDialogAsync(viewModel);
            if (emailTemplateDto == null)
            {
                return;
            }

            try
            {
                var result = await _emailDataService.AddEmailTemplateAsync(emailTemplateDto);
                _notificationService.ShowSuccess("添加邮件模板成功");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("添加邮件模板失败", ex);
            }
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

            var viewModel = App.Current.Services.GetRequiredService<EmailTemplateDialogViewModel>();
            viewModel.SetEmailTemplate(SelectedEmailTemplate);
            var emailTemplateDto = await _dialogService.ShowDialogAsync(viewModel);
            if (emailTemplateDto == null)
            {
                return;
            }

            try
            {
                var result = await _emailDataService.UpdateEmailTemplateAsync(SelectedEmailTemplate.Id, emailTemplateDto);
                if (result)
                {
                    _notificationService.ShowSuccess("更新邮件模板成功");
                }
                else
                {
                    _notificationService.ShowError("更新邮件模板失败");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("更新邮件模板失败", ex);
            }
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

            var confirmDialogViewModel = new ConfirmDialogViewModel(
                "确认删除",
                $"确定要删除邮件模板 {SelectedEmailTemplate.Name} 吗？", "删除");

            var confirmResult = await _dialogService.ShowDialogAsync(confirmDialogViewModel);

            if (confirmResult == true)
            {
                try
                {
                    var result = await _emailDataService.DeleteEmailTemplateAsync(SelectedEmailTemplate.Id);
                    if (result)
                    {
                        _notificationService.ShowSuccess("删除成功");
                    }
                    else
                    {
                        _notificationService.ShowError("删除失败");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("删除失败", ex);
                }
            }
        }
    }
}