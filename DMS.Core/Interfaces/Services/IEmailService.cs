using DMS.Core.Models;

namespace DMS.Core.Interfaces.Services
{
    /// <summary>
    /// 邮件服务接口
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 异步发送邮件
        /// </summary>
        /// <param name="message">邮件消息</param>
        /// <param name="account">邮件账户</param>
        /// <returns>发送是否成功</returns>
        Task<bool> SendEmailAsync(EmailMessage message, EmailAccount account);

        /// <summary>
        /// 异步接收邮件
        /// </summary>
        /// <param name="account">邮件账户</param>
        /// <param name="count">接收邮件数量</param>
        /// <returns>接收到的邮件列表</returns>
        Task<List<EmailMessage>> ReceiveEmailsAsync(EmailAccount account, int count = 10);

        /// <summary>
        /// 测试邮件账户连接
        /// </summary>
        /// <param name="account">邮件账户</param>
        /// <returns>连接是否成功</returns>
        Task<bool> TestConnectionAsync(EmailAccount account);
    }
}