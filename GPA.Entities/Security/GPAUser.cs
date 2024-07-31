using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPAUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastTOTPCode { get; set; }
        public byte TOTPAccessCodeAttempts { get; set; }
        public DateTimeOffset TOTPAccessCodeAttemptsDate { get; set; }
        public bool Deleted { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<GPAUserRole> UserRoles { get; set; }
        public ICollection<GPAUserToken> UserTokens { get; set; }
        public ICollection<GPAUserLogin> UserLogins { get; set; }
        public ICollection<GPAUserProfile> Profiles { get; set; }
    }
}
