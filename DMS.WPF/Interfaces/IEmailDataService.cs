using System.Collections.ObjectModel;
using DMS.Application.DTOs;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 邮件数据服务接口。
/// </summary>
public interface IEmailDataService
{
    /// <summary>
    /// 邮件账户列表。
    /// </summary>
    ObservableCollection<EmailAccountDto> EmailAccounts { get; set; }

    /// <summary>
    /// 邮件模板列表。
    /// </summary>
    ObservableCollection<EmailTemplateDto> EmailTemplates { get; set; }

    /// <summary>
    /// 加载所有邮件数据。
    /// </summary>
    void LoadAllEmailData();

    /// <summary>
    /// 添加邮件账户。
    /// </summary>
    Task<EmailAccountDto> AddEmailAccountAsync(CreateEmailAccountRequest request);

    /// <summary>
    /// 更新邮件账户。
    /// </summary>
    Task<bool> UpdateEmailAccountAsync(int id, CreateEmailAccountRequest request);

    /// <summary>
    /// 删除邮件账户。
    /// </summary>
    Task<bool> DeleteEmailAccountAsync(int id);

    /// <summary>
    /// 测试邮件账户连接。
    /// </summary>
    Task<bool> TestEmailAccountAsync(int id);

    /// <summary>
    /// 添加邮件模板。
    /// </summary>
    Task<EmailTemplateDto> AddEmailTemplateAsync(EmailTemplateDto template);

    /// <summary>
    /// 更新邮件模板。
    /// </summary>
    Task<bool> UpdateEmailTemplateAsync(int id, EmailTemplateDto template);

    /// <summary>
    /// 删除邮件模板。
    /// </summary>
    Task<bool> DeleteEmailTemplateAsync(int id);
}