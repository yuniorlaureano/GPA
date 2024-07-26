namespace GPA.Dtos.General
{
    public interface IEmailService
    {
        string Engine { get; }
        Task SendEmail(IGPAEmailMessage mailMessage, string options);
    }
}
