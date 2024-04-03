namespace GPA.Common.DTOs.Inventory
{
    public class StockCreationDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int TransactionType { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProviderId { get; set; }
        public Guid StoreId { get; set; }
        public int ReasonId { get; set; }
    }
}
