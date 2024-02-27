using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPAUserRole : IdentityUserRole<Guid>
    {
        public Guid Id { get; set; }
        public GPAUser User { get; set; }
        public GPARole Role { get; set; }
    }
}
