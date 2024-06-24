using GPA.Common.DTOs.Inventory;

namespace GPA.Common.DTOs.Unmapped
{
    public class ProductCatalogDto
    {
        public int Stock { get; set; }
        public int Input { get; set; }
        public int Output { get; set; }
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public byte ProductType { get; set; }
        public String ProductName { get; set; }
        public String ProductCode { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public AddonDto []Addons { get; set; }
    }
}
