using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// 邮件模板对话框视图模型
    /// </summary>
    public partial class EmailTemplateDialogViewModel : DialogViewModelBase<EmailTemplateDto>, IDialogViewModel
    {
        private readonly IEmailAppService _emailAppService;
        private readonly INotificationService _notificationService;
        private bool _isEditMode;
        private int _templateId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _subject = string.Empty;

        [ObservableProperty]
        private string _body = string.Empty;

        [ObservableProperty]
        private bool _isHtml = true;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string _dialogTitle = "添加邮件模板";

        public event Action<bool?>? CloseRequested;

        public EmailTemplateDialogViewModel(
            IEmailAppService emailAppService,
            INotificationService notificationService)
        {
            _emailAppService = emailAppService;
            _notificationService = notificationService;
            PrimaryButText = "保存";
        }

        /// <summary>
        /// 设置要编辑的邮件模板
        /// </summary>
        public void SetEmailTemplate(EmailTemplateDto template)
        {
            _isEditMode = true;
            _templateId = template.Id;
            DialogTitle = "编辑邮件模板";

            Name = template.Name;
            Code = template.Code;
            Subject = template.Subject;
            Body = template.Body;
            IsHtml = template.IsHtml;
            IsActive = template.IsActive;
        }

        /// <summary>
        /// 保存命令
        /// </summary>
        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateInput())
                return;

            try
            {
                var template = new EmailTemplateDto
                {
                    Name = Name,
                    Code = Code,
                    Subject = Subject,
                    Body = Body,
                    IsHtml = IsHtml,
                    IsActive = IsActive
                };

                if (_isEditMode)
                {
                    await _emailAppService.UpdateEmailTemplateAsync(_templateId, template);
                    _notificationService.ShowSuccess("邮件模板更新成功");
                }
                else
                {
                    await _emailAppService.CreateEmailTemplateAsync(template);
                    _notificationService.ShowSuccess("邮件模板创建成功");
                }

                CloseRequested?.Invoke(true);
                await Close(new EmailTemplateDto());
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(_isEditMode ? "更新失败" : "创建失败", ex);
            }
        }

        /// <summary>
        /// 取消命令
        /// </summary>
        [RelayCommand]
        private async Task Cancel()
        {
            CloseRequested?.Invoke(false);
            await Close(null);
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _notificationService.ShowWarn("请输入模板名称");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                _notificationService.ShowWarn("请输入模板代码");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                _notificationService.ShowWarn("请输入邮件主题");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Body))
            {
                _notificationService.ShowWarn("请输入邮件内容");
                return false;
            }

            return true;
        }
    }
}