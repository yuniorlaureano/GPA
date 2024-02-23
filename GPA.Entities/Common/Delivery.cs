using GPA.Common.Entities.Security;
using GPA.Entities;

namespace GPA.Common.Entities.Comon
{
    public class Delivery : Entity
    {
        public string Name { get; set; }
        public string LastName { get; set; }

        public Guid UserId { get; set; }
        public GPAUser User { get; set; }
    }
}
