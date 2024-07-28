﻿using GPA.Dtos.General;
using GPA.Utils;
using System.Net.Mail;

namespace GPA.Services.General.Email
{
    public class SmtpEmailService : IEmailService
    {
        public string Engine => EmailConstants.SMTP;

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
                throw;
            }
        }

        private async Task<SmtpClient> Configure(string options)
        {
            var smtpOptions = System.Text.Json.JsonSerializer.Deserialize<SmtpEmailOptions>(options, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            return new SmtpClient(smtpOptions.Host)
            {
                Port = smtpOptions.Port,
                Credentials = new System.Net.NetworkCredential(smtpOptions.UserName, smtpOptions.Password),
                EnableSsl = smtpOptions.UseSsl,
            };
        }
    }
}
