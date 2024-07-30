using GPA.Dtos.General;
using GPA.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace GPA.Services.General.Email
{
    public class SmtpEmailService : IEmailService
    {
        public string Engine => EmailConstants.SMTP;

        private readonly IEmailProviderHelper _emailProviderHelper;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IEmailProviderHelper emailProviderHelper, ILogger<SmtpEmailService> logger)
        {
            _emailProviderHelper = emailProviderHelper;
            _logger = logger;
        }

        public async Task SendEmail(IGPAEmailMessage mailMessage, string options)
        {
            try
            {
                var mgs = (SmtpEmailMessage)mailMessage;
                var smtpClient = await Configure(options);
                smtpClient.Send(mgs.GetMessage());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email message for provider: {Provider}, {Message}", Engine, ex.Message);
                throw;
            }
        }

        private async Task<SmtpClient> Configure(string options)
        {
            var smtpOptions = (SmtpEmailOptions)_emailProviderHelper.DecryptCredentialsInOptions(options, Engine);
            return new SmtpClient(smtpOptions.Host)
            {
                Port = smtpOptions.Port,
                Credentials = new System.Net.NetworkCredential(smtpOptions.UserName, smtpOptions.Password),
                EnableSsl = smtpOptions.UseSsl,
            };
        }
    }
}
