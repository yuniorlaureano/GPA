using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPARole : IdentityRole<Guid>
    {
        public ICollection<GPARoleClaim> RoleClaims { get; set; }
        public ICollection<GPAUserRole> UserRoles { get; set; }
    }
}
