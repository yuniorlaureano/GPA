namespace GPA.Common.DTOs.Inventory
{
    public class StockDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public int TransactionType { get; set; }

        public int Quantity { get; set; }

        public Guid ProductId { get; set; }
        public String ProductName { get; set; }
        public String ProductCode { get; set; }

        public Guid? ProviderId { get; set; }
        public string? ProviderName { get; set; }

        public Guid? StoreId { get; set; }
        public string? StoreName { get; set; }

        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
    }
}
