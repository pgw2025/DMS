using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.WPF.Interfaces;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// 邮件账户对话框视图模型
    /// </summary>
    public partial class EmailAccountDialogViewModel : DialogViewModelBase<CreateEmailAccountRequest>, IDialogViewModel
    {
        private readonly IEmailAppService _emailAppService;
        private readonly INotificationService _notificationService;
        private bool _isEditMode;
        private int _accountId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _emailAddress = string.Empty;

        [ObservableProperty]
        private string _smtpServer = string.Empty;

        [ObservableProperty]
        private int _smtpPort = 587;

        [ObservableProperty]
        private bool _enableSsl = true;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _imapServer = string.Empty;

        [ObservableProperty]
        private int _imapPort = 993;

        [ObservableProperty]
        private bool _isDefault;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string _dialogTitle = "添加邮件账户";

        public event Action<bool?>? CloseRequested;

        public EmailAccountDialogViewModel(
            IEmailAppService emailAppService,
            INotificationService notificationService)
        {
            _emailAppService = emailAppService;
            _notificationService = notificationService;
            PrimaryButText = "保存";
        }

        /// <summary>
        /// 设置要编辑的邮件账户
        /// </summary>
        public void SetEmailAccount(EmailAccountDto account)
        {
            _isEditMode = true;
            _accountId = account.Id;
            DialogTitle = "编辑邮件账户";

            Name = account.Name;
            EmailAddress = account.EmailAddress;
            SmtpServer = account.SmtpServer;
            SmtpPort = account.SmtpPort;
            EnableSsl = account.EnableSsl;
            Username = account.Username;
            Password = ""; // 出于安全考虑，不显示密码
            ImapServer = account.ImapServer ?? "";
            ImapPort = account.ImapPort;
            IsDefault = account.IsDefault;
            IsActive = account.IsActive;
        }

        /// <summary>
        /// 保存命令
        /// </summary>
        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateInput())
                return;

            var request = new CreateEmailAccountRequest
                          {
                              Name = Name,
                              EmailAddress = EmailAddress,
                              SmtpServer = SmtpServer,
                              SmtpPort = SmtpPort,
                              EnableSsl = EnableSsl,
                              Username = Username,
                              Password = Password,
                              ImapServer = string.IsNullOrEmpty(ImapServer) ? null : ImapServer,
                              ImapPort = ImapPort,
                              IsDefault = IsDefault,
                              IsActive = IsActive
                          };
            Close(request);
        }

        /// <summary>
        /// 取消命令
        /// </summary>
        [RelayCommand]
        private async Task Cancel()
        {
            Close(null);
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _notificationService.ShowWarn("请输入账户名称");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                _notificationService.ShowWarn("请输入邮箱地址");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SmtpServer))
            {
                _notificationService.ShowWarn("请输入SMTP服务器地址");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                _notificationService.ShowWarn("请输入用户名");
                return false;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(Password))
            {
                _notificationService.ShowWarn("请输入密码");
                return false;
            }

            return true;
        }
    }
}