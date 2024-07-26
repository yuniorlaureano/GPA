using System.Net.Mail;

namespace GPA.Dtos.Network
{
    public class SmtpEmailMessage : IGPAEmailMessage
    {
        public MailAddress From { get; set; }
        public MailAddressCollection To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }

        public SmtpEmailMessage(MailMessage mailMessage)
        {
            From = mailMessage.From;
            To = mailMessage.To;
            Subject = mailMessage.Subject;
            Body = mailMessage.Body;
            IsBodyHtml = mailMessage.IsBodyHtml;
        }

        public MailMessage GetMessage()
        {
            var mailMessage = new MailMessage
            {
                From = From,
                Subject = Subject,
                Body = Body,
                IsBodyHtml = IsBodyHtml
            };
            foreach (var address in To)
            {
                mailMessage.To.Add(address);
            }
            return mailMessage;
        }
    }
}
