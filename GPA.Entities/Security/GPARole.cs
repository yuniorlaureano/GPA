using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPARole : IdentityRole<Guid>
    {
        public string Name { get; set; }
        public string LastName { get; set; }

        public ICollection<GPARoleClaim> Claims { get; set; }
        public ICollection<GPAUserRole> UserRoles { get; set; }
    }
}
