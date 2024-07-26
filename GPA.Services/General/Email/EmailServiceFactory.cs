using GPA.Data.General;
using GPA.Dtos.General;
using GPA.Entities.General;
using GPA.Utils;
using System.Net.Mail;

namespace GPA.Services.General.Email
{
    public class EmailMessage
    {
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
    }

    public interface IEmailServiceFactory
    {
        Task SendMessageAsync(EmailMessage message);
    }

    public class EmailServiceFactory : IEmailServiceFactory
    {
        private readonly IEmailConfigurationRepository _emailConfigurationRepository;
        private readonly IEnumerable<IEmailService> _emailSender;

        public EmailServiceFactory(
            IEmailConfigurationRepository emailConfigurationRepository,
            IEnumerable<IEmailService> emailSender)
        {
            _emailConfigurationRepository = emailConfigurationRepository;
            _emailSender = emailSender;
        }

        public async Task SendMessageAsync(EmailMessage message)
        {
            var config = await _emailConfigurationRepository.GetByIdAsync(query => query, x => x.Current);
            if (config is null)
            {
                throw new ArgumentNullException("No existe configuración activa");
            }

            var mgs = GetMessage(message, config);
            var emailService = _emailSender.Where(x => x.Engine == config.Engine).FirstOrDefault();
            await emailService.SendEmail(mgs, config.Value);
        }

        private IGPAEmailMessage GetMessage(EmailMessage message, EmailConfiguration config)
        {
            switch (config.Engine)
            {
                case EmailConstants.SMTP:
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(config.From),
                        Subject = message.Subject,
                        Body = message.Body,
                        IsBodyHtml = message.IsBodyHtml
                    };

                    foreach (var address in message.To)
                    {
                        mailMessage.To.Add(address);
                    }

                    return new SmtpEmailMessage(mailMessage);
                default:
                    throw new ArgumentException("No se ha encontrado el proveedor de correo");
            }
        }
    }
}
