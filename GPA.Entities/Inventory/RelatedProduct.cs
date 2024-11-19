using GPA.Common.Entities.Inventory;

namespace GPA.Entities.Inventory
{
    public class RelatedProduct
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid RelatedProductId { get; set; }
        public int Quantity { get; set; }

        public Product Product { get; set; }
    }
}
