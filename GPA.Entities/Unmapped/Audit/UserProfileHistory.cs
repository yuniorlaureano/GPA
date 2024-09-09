using GPA.Common.Entities.Security;

namespace GPA.Entities.Unmapped.Audit
{
    public class UserProfileHistory : EntityHistory<Guid>
    {
        public Guid UserId { get; set; }
        public Guid ProfileId { get; set; }
    }
}
