namespace GPA.Common.DTOs.Inventory
{
    public class StockCreationDto
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public byte TransactionType { get; set; }
        public byte Status { get; set; }
        public Guid? ProviderId { get; set; }
        public Guid? StoreId { get; set; }
        public int ReasonId { get; set; }

        public IEnumerable<StockCreationDetailDto> StockDetails { get; set; }
    }

    public class StockCreationDetailDto
    {
        public Guid ProductId { get; set;}
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
    }

    public record DetailedDate(int Year, int Month, int Day);
}
