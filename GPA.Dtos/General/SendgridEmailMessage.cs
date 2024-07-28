using System.Net.Mail;

namespace GPA.Dtos.General
{
    public class SendGridEmailMessage : IGPAEmailMessage
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }

        public SendGridEmailMessage(MailMessage mailMessage)
        {
            From = mailMessage.From.Address;
            To = mailMessage.To.Select(x => x.Address).ToList();
            Subject = mailMessage.Subject;
            Body = mailMessage.Body;
            IsBodyHtml = mailMessage.IsBodyHtml;
        }

        public object GetMessage()
        {
            return new
            {
                personalizations = new List<object>
                {
                    new
                    {
                        to = To.Select(x => new { email = x }).ToList()                        
                    }
                },
                from = new
                {
                    email = From
                },
                subject = Subject,
                content = new List<object>
                {
                    new
                    {
                        type = IsBodyHtml ? "text/html" : "text/plain",
                        value = Body
                    }
                }
            };
        }
    }
}
