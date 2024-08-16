namespace GPA.Entities.Unmapped.Inventory
{
    public class RawStockCycleDetails
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public byte ProductType { get; set; }
        public int Stock { get; set; }
        public int Input { get; set; }
        public int Output { get; set; }
        public byte Type { get; set; }
        public Guid StockCycleId { get; set; }
    }
}
