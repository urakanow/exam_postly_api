using System.Net;
using System.Net.Mail;
using exam_postly_api.Interfaces;

namespace exam_postly_api.Utilities;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            // Credentials = new NetworkCredential("urakanow@gmail.com", "jfbg rwfl vumy mepm ")
            Credentials = new NetworkCredential("shukayka.exam@gmail.com", "jvml scyr szbm kwjo ")
        };

        return client.SendMailAsync(
            new MailMessage(from: "noreply@gmail.com",
                to: email,
                subject,
                message
            ));
    }
}