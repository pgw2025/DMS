using System.Collections.ObjectModel;
using System.Windows.Threading;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.WPF.Interfaces;

namespace DMS.WPF.Services;

/// <summary>
/// 邮件数据服务类，负责管理邮件相关的数据和操作。
/// </summary>
public class EmailDataService : IEmailDataService
{
    private readonly IMapper _mapper;
    private readonly IEmailAppService _emailAppService;
    private readonly INotificationService _notificationService;
    private readonly Dispatcher _uiDispatcher;

    /// <summary>
    /// 邮件账户列表。
    /// </summary>
    public ObservableCollection<EmailAccountDto> EmailAccounts { get; set; } = new();

    /// <summary>
    /// 邮件模板列表。
    /// </summary>
    public ObservableCollection<EmailTemplateDto> EmailTemplates { get; set; } = new();

    /// <summary>
    /// EmailDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="emailAppService">邮件应用服务实例。</param>
    /// <param name="notificationService">通知服务实例。</param>
    public EmailDataService(IMapper mapper, IEmailAppService emailAppService, INotificationService notificationService)
    {
        _mapper = mapper;
        _emailAppService = emailAppService;
        _notificationService = notificationService;
        _uiDispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    /// 加载所有邮件数据。
    /// </summary>
    public void LoadAllEmailData()
    {
        _ = LoadAllEmailDataAsync();
    }

    /// <summary>
    /// 异步加载所有邮件数据。
    /// </summary>
    private async Task LoadAllEmailDataAsync()
    {
        try
        {
            // 加载邮件账户
            var accounts = await _emailAppService.GetAllEmailAccountsAsync();
            _uiDispatcher.Invoke(() =>
            {
                EmailAccounts.Clear();
                foreach (var account in accounts)
                {
                    EmailAccounts.Add(account);
                }
            });

            // 加载邮件模板
            var templates = await _emailAppService.GetAllEmailTemplatesAsync();
            _uiDispatcher.Invoke(() =>
            {
                EmailTemplates.Clear();
                foreach (var template in templates)
                {
                    EmailTemplates.Add(template);
                }
            });
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("加载邮件数据失败", ex);
        }
    }

    /// <summary>
    /// 添加邮件账户。
    /// </summary>
    public async Task<EmailAccountDto> AddEmailAccountAsync(CreateEmailAccountRequest request)
    {
        var emailAccount = await _emailAppService.CreateEmailAccountAsync(request);
        _uiDispatcher.Invoke(() =>
        {
            EmailAccounts.Add(emailAccount);
        });
        return emailAccount;
    }

    /// <summary>
    /// 更新邮件账户。
    /// </summary>
    public async Task<bool> UpdateEmailAccountAsync(int id, CreateEmailAccountRequest request)
    {
        try
        {
            var emailAccount = await _emailAppService.UpdateEmailAccountAsync(id, request);
            _uiDispatcher.Invoke(() =>
            {
                var existingAccount = EmailAccounts.FirstOrDefault(a => a.Id == id);
                if (existingAccount != null)
                {
                    // 更新现有账户的信息
                    var index = EmailAccounts.IndexOf(existingAccount);
                    EmailAccounts[index] = emailAccount;
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("更新邮件账户失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 删除邮件账户。
    /// </summary>
    public async Task<bool> DeleteEmailAccountAsync(int id)
    {
        try
        {
            var result = await _emailAppService.DeleteEmailAccountAsync(id);
            if (result)
            {
                _uiDispatcher.Invoke(() =>
                {
                    var accountToRemove = EmailAccounts.FirstOrDefault(a => a.Id == id);
                    if (accountToRemove != null)
                    {
                        EmailAccounts.Remove(accountToRemove);
                    }
                });
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("删除邮件账户失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 测试邮件账户连接。
    /// </summary>
    public async Task<bool> TestEmailAccountAsync(int id)
    {
        try
        {
            return await _emailAppService.TestEmailAccountAsync(id);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("测试邮件账户连接失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 添加邮件模板。
    /// </summary>
    public async Task<EmailTemplateDto> AddEmailTemplateAsync(EmailTemplateDto template)
    {
        var emailTemplate = await _emailAppService.CreateEmailTemplateAsync(template);
        _uiDispatcher.Invoke(() =>
        {
            EmailTemplates.Add(emailTemplate);
        });
        return emailTemplate;
    }

    /// <summary>
    /// 更新邮件模板。
    /// </summary>
    public async Task<bool> UpdateEmailTemplateAsync(int id, EmailTemplateDto template)
    {
        try
        {
            var emailTemplate = await _emailAppService.UpdateEmailTemplateAsync(id, template);
            _uiDispatcher.Invoke(() =>
            {
                var existingTemplate = EmailTemplates.FirstOrDefault(t => t.Id == id);
                if (existingTemplate != null)
                {
                    // 更新现有模板的信息
                    var index = EmailTemplates.IndexOf(existingTemplate);
                    EmailTemplates[index] = emailTemplate;
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("更新邮件模板失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 删除邮件模板。
    /// </summary>
    public async Task<bool> DeleteEmailTemplateAsync(int id)
    {
        try
        {
            var result = await _emailAppService.DeleteEmailTemplateAsync(id);
            if (result)
            {
                _uiDispatcher.Invoke(() =>
                {
                    var templateToRemove = EmailTemplates.FirstOrDefault(t => t.Id == id);
                    if (templateToRemove != null)
                    {
                        EmailTemplates.Remove(templateToRemove);
                    }
                });
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError("删除邮件模板失败", ex);
            return false;
        }
    }
}