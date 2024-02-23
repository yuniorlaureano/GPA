using GPA.Entities;

namespace GPA.Common.Entities.Security
{
    public class GPAUser : Entity
    {
        public string Name { get; set; }
        public string LastName { get; set; }
    }
}
