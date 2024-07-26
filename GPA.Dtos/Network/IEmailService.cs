namespace GPA.Dtos.Network
{
    public interface IEmailService
    {
        string Engine { get; }
        Task SendEmail(IGPAEmailMessage mailMessage, string options);
    }
}
