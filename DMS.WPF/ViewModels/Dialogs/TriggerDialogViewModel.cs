using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models.Triggers;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// 触发器编辑器视图模型
    /// </summary>
    public partial class TriggerDialogViewModel : DialogViewModelBase<TriggerItem?>
    {

        private readonly IDialogService _dialogService;
        private readonly IWpfDataService _dataStorageService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private TriggerItem _trigger = new();

        // Properties for easier binding in XAML for SendEmail action config
        [ObservableProperty]
        private string _emailRecipients = "";

        [ObservableProperty]
        private string _emailSubjectTemplate = "";

        [ObservableProperty]
        private string _emailBodyTemplate = "";

        public TriggerDialogViewModel(
            IDialogService dialogService,
            IWpfDataService dataStorageService,
            INotificationService notificationService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _dataStorageService = dataStorageService;
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }



        /// <summary>
        /// 初始化视图模型（传入待编辑的触发器）
        /// </summary>
        /// <param name="parameter">待编辑的触发器 DTO</param>
        public async Task OnInitializedAsync(object? parameter)
        {
            if (parameter is TriggerItem triggerItemViewModel)
            {
                Trigger = triggerItemViewModel;
                Title = Trigger.Id == default(int) ? "新建触发器" : "编辑触发器";
                PrimaryButText = "保存";

                // Parse action configuration if it's SendEmail
                if (Trigger.Action == ActionType.SendEmail && !string.IsNullOrEmpty(Trigger.ActionConfigurationJson))
                {
                    try
                    {
                        var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Trigger.ActionConfigurationJson);
                        if (config != null)
                        {
                            if (config.TryGetValue("Recipients", out var recipientsElement))
                            {
                                var recipients = recipientsElement.Deserialize<List<string>>();
                                EmailRecipients = string.Join(";", recipients ?? new List<string>());
                            }
                            EmailSubjectTemplate = config.TryGetValue("SubjectTemplate", out var subjectElement) ? subjectElement.GetString() ?? "" : "";
                            EmailBodyTemplate = config.TryGetValue("BodyTemplate", out var bodyElement) ? bodyElement.GetString() ?? "" : "";
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowWarn($"无法解析邮件配置: {ex.Message}");
                    }
                }
            }
        }



        /// <summary>
        /// 保存按钮点击命令
        /// </summary>
        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Trigger.Name))
            {
                _notificationService.ShowWarn("请输入触发器名称");
                return;
            }

            if (string.IsNullOrWhiteSpace(Trigger.Description))
            {
                _notificationService.ShowWarn("请输入触发器描述");
                return;
            }



            // Prepare action configuration based on selected action type
            if (Trigger.Action == ActionType.SendEmail)
            {
                if (string.IsNullOrWhiteSpace(EmailRecipients))
                {
                    _notificationService.ShowWarn("请输入至少一个收件人邮箱地址");
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmailSubjectTemplate))
                {
                    _notificationService.ShowWarn("请输入邮件主题模板");
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmailBodyTemplate))
                {
                    _notificationService.ShowWarn("请输入邮件内容模板");
                    return;
                }

                var recipientList = new List<string>(EmailRecipients.Split(';', StringSplitOptions.RemoveEmptyEntries));
                var configDict = new Dictionary<string, object>
                {
                    { "Recipients", recipientList },
                    { "SubjectTemplate", EmailSubjectTemplate },
                    { "BodyTemplate", EmailBodyTemplate }
                };
                Trigger.ActionConfigurationJson = JsonSerializer.Serialize(configDict);
            }
            else
            {
                // For other actions, leave ActionConfigurationJson as is or set to default "{}"
                Trigger.ActionConfigurationJson ??= "{}";
            }

            // Set timestamps
            Trigger.UpdatedAt = DateTime.Now;
            if (Trigger.Id == default(int))
            {
                Trigger.CreatedAt = DateTime.Now;
                Trigger.Id = 0; // 对于自增ID，设置为0让数据库自动生成
            }

            // Close dialog with the updated trigger DTO
            await Close(Trigger);
        }

        /// <summary>
        /// 取消按钮点击命令
        /// </summary>
        [RelayCommand]
        private async Task CancelAsync()
        {
            await Close(null); // Return null to indicate cancellation
        }
    }
}