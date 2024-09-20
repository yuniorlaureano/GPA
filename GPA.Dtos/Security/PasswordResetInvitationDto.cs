namespace GPA.Dtos.Security
{
    public class PasswordResetInvitationDto
    {
        public string userId { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
