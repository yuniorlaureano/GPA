using GPA.Dtos.Network;
using System.Net.Mail;

namespace GPA.Services.Network.Email
{
    public class SmtpEmailService : IEmailService
    {
        public string Engine => "SMTP";
        private SmtpClient SmtpClient;

        public async Task SendEmail(IGPAEmailMessage mailMessage, string options)
        {
            try
            {
                var mgs = (SmtpEmailMessage)mailMessage;
                await Configure(options);
                SmtpClient.Send(mgs.GetMessage());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task Configure(string options)
        {
            var smtpOptions = System.Text.Json.JsonSerializer.Deserialize<SmtpEmailOptions>(options);
            var SmtpClient = new SmtpClient(smtpOptions.Host)
            {
                Port = smtpOptions.Port,
                Credentials = new System.Net.NetworkCredential(smtpOptions.UserName, smtpOptions.Password),
                EnableSsl = smtpOptions.UseSsl,
            };
        }
    }
}
