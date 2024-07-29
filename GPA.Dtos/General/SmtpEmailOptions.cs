namespace GPA.Dtos.General
{
    public class SmtpEmailOptions : IEmailOptions
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
    }
}
