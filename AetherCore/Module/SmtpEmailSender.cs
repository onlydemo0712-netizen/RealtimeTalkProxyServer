using AetherCore.Module.Interface;
using AetherCore.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AetherCore.Module
{
    public class SmtpEmailSender: ISmtpEmailSender
    {
        private readonly MailSenderSettings _opt;
        public SmtpEmailSender(IOptions<MailSenderSettings> opt)
        {
            _opt = opt.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string textBody, string? htmlBody = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_opt.FromMail) 
                || string.IsNullOrEmpty(_opt.FromName)
                || string.IsNullOrEmpty(_opt.Host))
                throw new InvalidOperationException("MailSettings is not configured.");

            // 1) SMTP 伺服器參數
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromMail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            // 2) 郵件內容
            var body = new BodyBuilder
            {
                TextBody = textBody,
                HtmlBody = htmlBody
            };
            msg.Body = body.ToMessageBody();

            // 3) 寄信
            using var smtp = new SmtpClient();

            // 關閉「憑證撤銷檢查」(避免 CRL/OCSP 查不到就炸)
            smtp.CheckCertificateRevocation = false;

            // 587: StartTLS (最常見)
            await smtp.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_opt.UserName, _opt.Password, ct);

            await smtp.SendAsync(msg, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
