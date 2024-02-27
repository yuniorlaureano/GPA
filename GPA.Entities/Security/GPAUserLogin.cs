using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPAUserLogin : IdentityUserLogin<Guid>
    {
        public Guid Id { get; set; }
        public GPAUser User { get; set; }        
    }
}
