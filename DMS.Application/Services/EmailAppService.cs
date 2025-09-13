using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;

namespace DMS.Application.Services
{
    /// <summary>
    /// 邮件应用服务实现
    /// </summary>
    public class EmailAppService : IEmailAppService
    {
        private readonly IEmailAccountRepository _emailAccountRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IEmailMessageRepository _emailMessageRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public EmailAppService(
            IEmailAccountRepository emailAccountRepository,
            IEmailTemplateRepository emailTemplateRepository,
            IEmailMessageRepository emailMessageRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _emailAccountRepository = emailAccountRepository;
            _emailTemplateRepository = emailTemplateRepository;
            _emailMessageRepository = emailMessageRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取所有邮件账户
        /// </summary>
        public async Task<List<EmailAccountDto>> GetAllEmailAccountsAsync()
        {
            var accounts = await _emailAccountRepository.GetAllAsync();
            return _mapper.Map<List<EmailAccountDto>>(accounts);
        }

        /// <summary>
        /// 根据ID获取邮件账户
        /// </summary>
        public async Task<EmailAccountDto> GetEmailAccountByIdAsync(int id)
        {
            var account = await _emailAccountRepository.GetByIdAsync(id);
            return _mapper.Map<EmailAccountDto>(account);
        }

        /// <summary>
        /// 创建邮件账户
        /// </summary>
        public async Task<EmailAccountDto> CreateEmailAccountAsync(CreateEmailAccountRequest request)
        {
            var account = _mapper.Map<EmailAccount>(request);
            var createdAccount = await _emailAccountRepository.AddAsync(account);
            return _mapper.Map<EmailAccountDto>(createdAccount);
        }

        /// <summary>
        /// 更新邮件账户
        /// </summary>
        public async Task<EmailAccountDto> UpdateEmailAccountAsync(int id, CreateEmailAccountRequest request)
        {
            var existingAccount = await _emailAccountRepository.GetByIdAsync(id);
            if (existingAccount == null)
                throw new ArgumentException($"未找到ID为 {id} 的邮件账户");

            _mapper.Map(request, existingAccount);
            existingAccount.UpdatedAt = DateTime.Now;

            var updatedAccount = await _emailAccountRepository.UpdateAsync(existingAccount);
            return _mapper.Map<EmailAccountDto>(existingAccount);
        }

        /// <summary>
        /// 删除邮件账户
        /// </summary>
        public async Task<bool> DeleteEmailAccountAsync(int id)
        {
            var account = await _emailAccountRepository.GetByIdAsync(id);
            if (account == null)
                return false;

            await _emailAccountRepository.DeleteAsync(account);
            return true;
        }

        /// <summary>
        /// 测试邮件账户连接
        /// </summary>
        public async Task<bool> TestEmailAccountAsync(int id)
        {
            var account = await _emailAccountRepository.GetByIdAsync(id);
            if (account == null)
                return false;

            return await _emailService.TestConnectionAsync(account);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public async Task<bool> SendEmailAsync(SendEmailRequest request)
        {
            var account = await _emailAccountRepository.GetByIdAsync(request.EmailAccountId);
            if (account == null)
                throw new ArgumentException($"未找到ID为 {request.EmailAccountId} 的邮件账户");

            var message = _mapper.Map<EmailMessage>(request);
            message.From = account.EmailAddress;

            return await _emailService.SendEmailAsync(message, account);
        }

        /// <summary>
        /// 获取所有邮件模板
        /// </summary>
        public async Task<List<EmailTemplateDto>> GetAllEmailTemplatesAsync()
        {
            var templates = await _emailTemplateRepository.GetAllAsync();
            return _mapper.Map<List<EmailTemplateDto>>(templates);
        }

        /// <summary>
        /// 根据ID获取邮件模板
        /// </summary>
        public async Task<EmailTemplateDto> GetEmailTemplateByIdAsync(int id)
        {
            var template = await _emailTemplateRepository.GetByIdAsync(id);
            return _mapper.Map<EmailTemplateDto>(template);
        }

        /// <summary>
        /// 根据代码获取邮件模板
        /// </summary>
        public async Task<EmailTemplateDto> GetEmailTemplateByCodeAsync(string code)
        {
            var template = await _emailTemplateRepository.GetByCodeAsync(code);
            return _mapper.Map<EmailTemplateDto>(template);
        }

        /// <summary>
        /// 创建邮件模板
        /// </summary>
        public async Task<EmailTemplateDto> CreateEmailTemplateAsync(EmailTemplateDto templateDto)
        {
            var template = _mapper.Map<EmailTemplate>(templateDto);
            var createdTemplate = await _emailTemplateRepository.AddAsync(template);
            return _mapper.Map<EmailTemplateDto>(createdTemplate);
        }

        /// <summary>
        /// 更新邮件模板
        /// </summary>
        public async Task<EmailTemplateDto> UpdateEmailTemplateAsync(int id, EmailTemplateDto templateDto)
        {
            var existingTemplate = await _emailTemplateRepository.GetByIdAsync(id);
            if (existingTemplate == null)
                throw new ArgumentException($"未找到ID为 {id} 的邮件模板");

            _mapper.Map(templateDto, existingTemplate);
            existingTemplate.UpdatedAt = DateTime.Now;

            var updatedTemplate = await _emailTemplateRepository.UpdateAsync(existingTemplate);
            return _mapper.Map<EmailTemplateDto>(existingTemplate);
        }

        /// <summary>
        /// 删除邮件模板
        /// </summary>
        public async Task<bool> DeleteEmailTemplateAsync(int id)
        {
            var template = await _emailTemplateRepository.GetByIdAsync(id);
            if (template == null)
                return false;

            await _emailTemplateRepository.DeleteAsync(template);
            return true;
        }
    }
}