namespace GPA.Entities.Unmapped.Audit
{
    public class StockDetailHistory
    {
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid ProductId { get; set; }
    }
}
