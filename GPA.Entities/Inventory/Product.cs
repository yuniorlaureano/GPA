using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Product : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal UnitCost { get; set; }
        public string? Description { get; set; }
        public decimal? WholesaleCost { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public Guid? ProductLocationId { get; set; }
        public ProductLocation? ProductLocation { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}
