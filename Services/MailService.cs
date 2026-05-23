using Microsoft.Extensions.Options;
using MimeKit;
using MyStore.config;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
namespace MyStore.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
        }

        public bool SendMail(MailData mailData)
        {
            try
            {
                // Tạo email mới
                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId));
                emailMessage.To.Add(new MailboxAddress(mailData.EmailToName, mailData.EmailToId));
                emailMessage.Subject = mailData.EmailSubject;

                // Thiết lập nội dung email (HTML)
                BodyBuilder emailBodyBuilder = new BodyBuilder
                {
                    HtmlBody = mailData.EmailBody 
                };
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                // Khởi tạo SMTP Client và gửi mail
                using (SmtpClient mailClient = new SmtpClient())
                {
                    mailClient.Connect(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSSL);
                    mailClient.Authenticate(_mailSettings.EmailId, _mailSettings.Password);
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi email: " + ex.Message);
                return false;
            }
        }

    }
}
