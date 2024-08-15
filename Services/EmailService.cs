using ChitChat.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ChitChat.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email"); //This is the sender of the email(ChitChatApp)
            MimeMessage newEmail = new();//allows us to create new messager and send them with mailkit

            //From:
            newEmail.Sender = MailboxAddress.Parse(emailSender);

            //To:
            foreach (var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }

            //Subject
            newEmail.Subject = subject;

            //Body
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();
            //Assembles the body of the email.

            //We need to login to our smtp client
            using SmtpClient smtpClient = new();//using MailKit.Net.Smtp;
            try
            {
                var host = _mailSettings.MailHost ?? Environment.GetEnvironmentVariable("MailHost");
                var port = _mailSettings.MailPort != 0 ? _mailSettings.MailPort : int.Parse(Environment.GetEnvironmentVariable("Port")!);
                var password = _mailSettings.MailPassword ?? Environment.GetEnvironmentVariable("MailPassword");

                //connect
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                //Authenticate
                await smtpClient.AuthenticateAsync(emailSender, password);
                //Send               
                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }

        }
    }
}
