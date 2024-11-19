using GPA.Entities;
using GPA.Entities.General;
using GPA.Entities.Inventory;

namespace GPA.Common.Entities.Inventory
{
    public class Product : Entity<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public ProductType Type { get; set; }
        public double UnitValue { get; set; }
        public Guid UnitId { get; set; }
        public Unit Unit { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public Guid? ProductLocationId { get; set; }
        public ProductLocation? ProductLocation { get; set; }

        public ICollection<StockDetails> Stocks { get; set; }
        public ICollection<ProductAddon> ProductAddons { get; set; }
        public ICollection<RelatedProduct> RelatedProducts { get; set; }
    }
}
