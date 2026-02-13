namespace AetherCore.Module.Interface
{
    public interface ISmtpEmailSender
    {
        Task SendAsync(string toEmail, string subject, string textBody, string? htmlBody = null, CancellationToken ct = default);
    }
}
