namespace GPA.Common.DTOs.Inventory
{
    public class StockWithDetailDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public DetailedDate Date { get; set; }
        public int TransactionType { get; set; }
        public int Status { get; set; }
        public Guid? ProviderId { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderRnc { get; set; }
        public Guid? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        public IEnumerable<StockDetailsDto> StockDetails { get; set; }
    }
}
