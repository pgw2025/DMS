using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Services
{
    /// <summary>
    /// 邮件功能测试服务
    /// </summary>
    public class EmailFunctionalityTestService
    {
        private readonly IEmailAppService _emailAppService;
        private readonly IEmailAccountRepository _emailAccountRepository;
        private readonly IEmailService _emailService;

        public EmailFunctionalityTestService(
            IEmailAppService emailAppService,
            IEmailAccountRepository emailAccountRepository,
            IEmailService emailService)
        {
            _emailAppService = emailAppService;
            _emailAccountRepository = emailAccountRepository;
            _emailService = emailService;
        }

        /// <summary>
        /// 运行邮件功能测试
        /// </summary>
        public async Task RunTestAsync()
        {
            try
            {
                // 1. 创建测试邮件账户
                var createRequest = new CreateEmailAccountRequest
                {
                    Name = "测试邮件账户",
                    EmailAddress = "test@example.com",
                    SmtpServer = "smtp.example.com",
                    SmtpPort = 587,
                    EnableSsl = true,
                    Username = "test@example.com",
                    Password = "password",
                    ImapServer = "imap.example.com",
                    ImapPort = 993,
                    IsDefault = true,
                    IsActive = true
                };

                var createdAccount = await _emailAppService.CreateEmailAccountAsync(createRequest);
                Console.WriteLine($"创建邮件账户成功，ID: {createdAccount.Id}");

                // 2. 获取所有邮件账户
                var accounts = await _emailAppService.GetAllEmailAccountsAsync();
                Console.WriteLine($"获取到 {accounts.Count} 个邮件账户");

                // 3. 测试连接（这会失败，因为我们使用的是假的服务器地址）
                var connectionResult = await _emailAppService.TestEmailAccountAsync(createdAccount.Id);
                Console.WriteLine($"邮件账户连接测试结果: {connectionResult}");

                // 4. 创建测试邮件模板
                var templateDto = new EmailTemplateDto
                {
                    Name = "测试模板",
                    Code = "TEST_TEMPLATE",
                    Subject = "测试邮件主题",
                    Body = "<h1>测试邮件内容</h1><p>这是一封测试邮件。</p>",
                    IsHtml = true,
                    IsActive = true
                };

                var createdTemplate = await _emailAppService.CreateEmailTemplateAsync(templateDto);
                Console.WriteLine($"创建邮件模板成功，ID: {createdTemplate.Id}");

                // 5. 获取所有邮件模板
                var templates = await _emailAppService.GetAllEmailTemplatesAsync();
                Console.WriteLine($"获取到 {templates.Count} 个邮件模板");

                Console.WriteLine("邮件功能测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"邮件功能测试过程中发生错误: {ex.Message}");
            }
        }
    }
}