using GPA.Entities;

namespace GPA.Common.Entities.Security
{
    public class GPAUserProfile : Entity<Guid>
    {
        public Guid UserId { get; set; }
        public GPAUser User { get; set; }

        public Guid ProfileId { get; set; }
        public GPAProfile Profile { get; set; }
    }
}
