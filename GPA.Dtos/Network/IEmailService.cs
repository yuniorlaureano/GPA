namespace GPA.Dtos.Network
{
    public interface IEmailService
    {
        string Provider { get; }
        Task SendEmail(IGPAEmailMessage mailMessage, string options);
    }
}
