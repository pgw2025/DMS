using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 邮件服务实现
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 异步发送邮件
        /// </summary>
        public async Task<bool> SendEmailAsync(EmailMessage message, EmailAccount account)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(account.EmailAddress, account.EmailAddress));
                mimeMessage.To.AddRange(ParseAddresses(message.To));
                
                if (!string.IsNullOrEmpty(message.Cc))
                    mimeMessage.Cc.AddRange(ParseAddresses(message.Cc));
                
                if (!string.IsNullOrEmpty(message.Bcc))
                    mimeMessage.Bcc.AddRange(ParseAddresses(message.Bcc));

                mimeMessage.Subject = message.Subject;

                var bodyBuilder = new BodyBuilder();
                if (message.IsHtml)
                    bodyBuilder.HtmlBody = message.Body;
                else
                    bodyBuilder.TextBody = message.Body;

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(account.SmtpServer, account.SmtpPort, account.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
                await client.AuthenticateAsync(account.Username, account.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);

                message.Status = EmailSendStatus.Sent;
                message.SentAt = DateTime.Now;
                
                _logger.LogInformation($"邮件发送成功: {message.Subject}");
                return true;
            }
            catch (Exception ex)
            {
                message.Status = EmailSendStatus.Failed;
                _logger.LogError(ex, $"邮件发送失败: {message.Subject}");
                return false;
            }
        }

        /// <summary>
        /// 异步接收邮件
        /// </summary>
        public async Task<List<EmailMessage>> ReceiveEmailsAsync(EmailAccount account, int count = 10)
        {
            var emails = new List<EmailMessage>();

            try
            {
                if (string.IsNullOrEmpty(account.ImapServer))
                {
                    _logger.LogWarning("未配置IMAP服务器，无法接收邮件");
                    return emails;
                }

                using var client = new ImapClient();
                await client.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(account.Username, account.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

                var endIndex = Math.Min(count, inbox.Count);
                for (int i = 0; i < endIndex; i++)
                {
                    var message = await inbox.GetMessageAsync(i);
                    var email = new EmailMessage
                    {
                        EmailAccountId = account.Id,
                        From = message.From.ToString(),
                        To = message.To.ToString(),
                        Subject = message.Subject,
                        Body = message.TextBody ?? message.HtmlBody ?? "",
                        IsHtml = !string.IsNullOrEmpty(message.HtmlBody),
                        SentAt = message.Date.DateTime,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    emails.Add(email);
                }

                await client.DisconnectAsync(true);
                _logger.LogInformation($"成功接收 {emails.Count} 封邮件");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收邮件时发生错误");
            }

            return emails;
        }

        /// <summary>
        /// 测试邮件账户连接
        /// </summary>
        public async Task<bool> TestConnectionAsync(EmailAccount account)
        {
            try
            {
                // 测试SMTP连接
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(account.SmtpServer, account.SmtpPort, account.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
                await smtpClient.AuthenticateAsync(account.Username, account.Password);
                await smtpClient.DisconnectAsync(true);

                // 如果配置了IMAP服务器，也测试IMAP连接
                if (!string.IsNullOrEmpty(account.ImapServer))
                {
                    using var imapClient = new ImapClient();
                    await imapClient.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect);
                    await imapClient.AuthenticateAsync(account.Username, account.Password);
                    await imapClient.DisconnectAsync(true);
                }

                _logger.LogInformation($"邮件账户 {account.EmailAddress} 连接测试成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"邮件账户 {account.EmailAddress} 连接测试失败");
                return false;
            }
        }

        /// <summary>
        /// 解析邮箱地址字符串
        /// </summary>
        private IEnumerable<MailboxAddress> ParseAddresses(string addresses)
        {
            var addressList = addresses.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var address in addressList)
            {
                var trimmedAddress = address.Trim();
                if (!string.IsNullOrEmpty(trimmedAddress))
                {
                    yield return MailboxAddress.Parse(trimmedAddress);
                }
            }
        }
    }
}