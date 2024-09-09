using GPA.Common.Entities.Security;

namespace GPA.Entities.Unmapped.Audit
{
    public class UserHistory : EntityHistory<Guid>
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
