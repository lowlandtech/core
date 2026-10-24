namespace LowlandTech.Core;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
