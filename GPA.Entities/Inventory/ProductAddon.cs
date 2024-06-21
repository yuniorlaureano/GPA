using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class ProductAddon : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid AddonId { get; set; }
        public Addon Addon { get; set; }
    }
}
