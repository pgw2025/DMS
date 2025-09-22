using System.Collections.ObjectModel;
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
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs
{
    /// <summary>
    /// 触发器编辑器视图模型
    /// </summary>
    public partial class TriggerDialogViewModel : DialogViewModelBase<TriggerDefinitionDto?>
    {
        private readonly IVariableAppService _variableAppService; // To populate variable selection dropdown
        private readonly IDialogService _dialogService;
        private readonly IDataStorageService _dataStorageService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private TriggerDefinitionDto _trigger = new();

        [ObservableProperty]
        private List<VariableDto> _availableVariables = new();
        
        [ObservableProperty]
        private ObservableCollection<VariableItemViewModel> _selectedVariables = new();

        // Properties for easier binding in XAML for SendEmail action config
        [ObservableProperty]
        private string _emailRecipients = "";

        [ObservableProperty]
        private string _emailSubjectTemplate = "";

        [ObservableProperty]
        private string _emailBodyTemplate = "";

        public TriggerDialogViewModel(
            IVariableAppService variableAppService,
            IDialogService dialogService,
            IDataStorageService dataStorageService,
            INotificationService notificationService)
        {
            _variableAppService = variableAppService ?? throw new ArgumentNullException(nameof(variableAppService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _dataStorageService = dataStorageService;
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        partial void OnSearchTextChanged(string searchText)
        {
            SelectedVariables.Clear();
            foreach (var variableKv in _dataStorageService.Variables)
            {
                if (variableKv.Value.Name.Contains(SearchText))
                {
                    SelectedVariables.Add(variableKv.Value);
                }
            }
        }
        
        

        /// <summary>
        /// 初始化视图模型（传入待编辑的触发器）
        /// </summary>
        /// <param name="parameter">待编辑的触发器 DTO</param>
        public async Task OnInitializedAsync(object? parameter)
        {
            if (parameter is TriggerDefinitionDto triggerDto)
            {
                Trigger = triggerDto;
                Title = Trigger.Id == default(int) ? "新建触发器" : "编辑触发器";
                PrimaryButText = "保存";
                
                // Load available variables for selection dropdown
                await LoadVariablesAsync();
                
                // Load selected variables
                if (Trigger.VariableIds != null && Trigger.VariableIds.Any())
                {
                    foreach (var variableId in Trigger.VariableIds)
                    {
                        var variable = AvailableVariables.FirstOrDefault(v => v.Id == variableId);
                        if (variable != null)
                        {
                            // SelectedVariables.Add(variable);
                        }
                    }
                }

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
        /// 加载所有可用的变量
        /// </summary>
        private async Task LoadVariablesAsync()
        {
            try
            {
                var variables = await _variableAppService.GetAllVariablesAsync();
                AvailableVariables = variables ?? new List<VariableDto>();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"加载变量列表失败: {ex.Message}");
                AvailableVariables = new List<VariableDto>();
            }
        }

        /// <summary>
        /// 保存按钮点击命令
        /// </summary>
        [RelayCommand]
        private async Task SaveAsync()
        {
            // Basic validation
            if (SelectedVariables == null || !SelectedVariables.Any())
            {
                _notificationService.ShowWarn("请至少选择一个关联的变量");
                return;
            }

            if (string.IsNullOrWhiteSpace(Trigger.Description))
            {
                _notificationService.ShowWarn("请输入触发器描述");
                return;
            }

            // 设置选中的变量ID
            Trigger.VariableIds = SelectedVariables.Select(v => v.Id).ToList();

            // Validate condition-specific fields
            switch (Trigger.Condition)
            {
                case ConditionType.GreaterThan:
                case ConditionType.LessThan:
                case ConditionType.EqualTo:
                case ConditionType.NotEqualTo:
                    if (!Trigger.Threshold.HasValue)
                    {
                        _notificationService.ShowWarn($"{Trigger.Condition} 条件需要设置阈值");
                        return;
                    }
                    break;
                case ConditionType.InRange:
                case ConditionType.OutOfRange:
                    if (!Trigger.LowerBound.HasValue || !Trigger.UpperBound.HasValue)
                    {
                        _notificationService.ShowWarn($"{Trigger.Condition} 条件需要设置下限和上限");
                        return;
                    }
                    if (Trigger.LowerBound > Trigger.UpperBound)
                    {
                        _notificationService.ShowWarn("下限必须小于或等于上限");
                        return;
                    }
                    break;
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
            Trigger.UpdatedAt = DateTime.UtcNow;
            if (Trigger.Id == default(int))
            {
                Trigger.CreatedAt = DateTime.UtcNow;
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