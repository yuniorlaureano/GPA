using Microsoft.AspNetCore.Identity;

namespace GPA.Common.Entities.Security
{
    public class GPAUserToken : IdentityUserToken<Guid>
    {
        public Guid Id { get; set; }
        public GPAUser User { get; set; }
    }
}
