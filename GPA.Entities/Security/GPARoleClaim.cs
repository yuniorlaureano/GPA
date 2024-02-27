using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPARoleClaim : IdentityRoleClaim<Guid>
    {
        public GPARole Role { get; set; }
    }
}
