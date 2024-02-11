namespace GPA.Common.DTOs.Inventory
{
    public class StockDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public int Input { get; set; }
        public int OutInput { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProviderId { get; set; }

        public ProductDto Product { get; set; }
        public ProviderDto Provider { get; set; }
    }
}
