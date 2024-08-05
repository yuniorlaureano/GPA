namespace GPA.Entities.Unmapped.Inventory
{
    public class RawStockDetails
    {
        public Guid? Id { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid ProductId { get; set; }
        public Guid StockId { get; set; }
    }
}
