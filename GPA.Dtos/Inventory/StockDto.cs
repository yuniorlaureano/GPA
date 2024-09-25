namespace GPA.Common.DTOs.Inventory
{
    public class StockDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public byte TransactionType { get; set; }
        public byte Status { get; set; }
        public Guid? ProviderId { get; set; }
        public string? ProviderIdentification { get; set; }
        public string? ProviderName { get; set; }
        public Guid? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public Guid? InvoiceId { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
    }
}
