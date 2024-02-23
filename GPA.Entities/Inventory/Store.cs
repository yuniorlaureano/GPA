using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Store : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string? Description { get; set; }
        public string Location { get; set; }
    }
}
