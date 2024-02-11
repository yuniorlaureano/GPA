using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class ProductLocation : Entity
    {
        public string? Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
