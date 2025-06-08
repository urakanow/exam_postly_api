namespace exam_postly_api.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}