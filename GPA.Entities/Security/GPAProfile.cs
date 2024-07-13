using GPA.Entities;

namespace GPA.Common.Entities.Security
{
    public class GPAProfile : Entity<Guid>
    {
        public string Name { get; set; }
        public string? Value { get; set; }

        public ICollection<GPAUserProfile> Users { get; set; }
    }
}
