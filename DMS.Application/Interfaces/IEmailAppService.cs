using DMS.Application.DTOs;

namespace DMS.Application.Interfaces
{
    /// <summary>
    /// 邮件应用服务接口
    /// </summary>
    public interface IEmailAppService
    {
        /// <summary>
        /// 获取所有邮件账户
        /// </summary>
        Task<List<EmailAccountDto>> GetAllEmailAccountsAsync();

        /// <summary>
        /// 根据ID获取邮件账户
        /// </summary>
        Task<EmailAccountDto> GetEmailAccountByIdAsync(int id);

        /// <summary>
        /// 创建邮件账户
        /// </summary>
        Task<EmailAccountDto> CreateEmailAccountAsync(CreateEmailAccountRequest request);

        /// <summary>
        /// 更新邮件账户
        /// </summary>
        Task<EmailAccountDto> UpdateEmailAccountAsync(int id, CreateEmailAccountRequest request);

        /// <summary>
        /// 删除邮件账户
        /// </summary>
        Task<bool> DeleteEmailAccountAsync(int id);

        /// <summary>
        /// 测试邮件账户连接
        /// </summary>
        Task<bool> TestEmailAccountAsync(int id);

        /// <summary>
        /// 发送邮件
        /// </summary>
        Task<bool> SendEmailAsync(SendEmailRequest request);

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync();

        /// <summary>
        /// 根据ID获取邮件模板
        /// </summary>
        Task<EmailTemplateDto> GetEmailTemplateByIdAsync(int id);

        /// <summary>
        /// 根据代码获取邮件模板
        /// </summary>
        Task<EmailTemplateDto> GetEmailTemplateByCodeAsync(string code);

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        Task<EmailTemplateDto> CreateEmailTemplateAsync(EmailTemplateDto template);

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        Task<EmailTemplateDto> UpdateEmailTemplateAsync(int id, EmailTemplateDto template);

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        Task<bool> DeleteEmailTemplateAsync(int id);
    }
}