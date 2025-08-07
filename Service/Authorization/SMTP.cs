using System.Net.Mail;
using System.Net;

namespace CRMService.Service.Authorization
{
    public class SMTP(ILoggerFactory logger)
    {
        private readonly ILogger<SMTP> _logger = logger.CreateLogger<SMTP>();

        public MailMessage? CreateMail(string name, string emailFrom, string emailTo, string subject, string body)
        {
            try
            {
                var from = new MailAddress(emailFrom, name);
                var to = new MailAddress(emailTo);
                var mail = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                return mail;
            }
            catch (Exception)
            {
                _logger.LogError("[Method:{MethodName}] Error while create mail.", nameof(CreateMail));
                return null;
            }
        }

        public bool SendMail(string host, int smtpPort, string emailFrom, string pass, MailMessage? mail)
        {
            if (mail == null) return false;
            try
            {
                using SmtpClient smtp = new(host, smtpPort);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(emailFrom, pass);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;

                smtp.Send(mail);
                return true;
            }
            catch (Exception)
            {
                _logger.LogError("[Method:{MethodName}] Error while send mail.", nameof(SendMail));
                return false;
            }
        }
    }
}
