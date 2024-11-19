namespace GPA.Entities.Unmapped.Inventory
{
    public class RawRelatedProduct
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid RelatedProductId { get; set; }
        public int Quantity { get; set; }
    }
}
