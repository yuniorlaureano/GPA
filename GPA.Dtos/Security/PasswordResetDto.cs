namespace GPA.Dtos.Security
{
    public class PasswordResetDto
    {
        public string userName { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
