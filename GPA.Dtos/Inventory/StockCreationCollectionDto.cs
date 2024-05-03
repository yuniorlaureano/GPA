namespace GPA.Common.DTOs.Inventory
{
    public class StockCreationCollectionDto
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public int TransactionType { get; set; }
        public Guid? ProviderId { get; set; }
        public Guid? StoreId { get; set; }
        public int ReasonId { get; set; }
        public DetailedDate Date { get; set; }

        public IEnumerable<StockCreationCollectionDetailDto> Products { get; set; }

        public IEnumerable<StockCreationDto> AsStockCreation()
        {
            return this.Products.Select(x => new StockCreationDto
            {
                Id = Id,
                Description = Description,
                TransactionType = TransactionType,
                ProviderId = ProviderId,
                StoreId = StoreId,
                ReasonId = ReasonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
            });
        }
    }

    public class StockCreationCollectionDetailDto
    {
        public Guid ProductId { get; set;}
        public int Quantity { get; set; }
    }

    public record DetailedDate(int Year, int Month, int Day);
}
