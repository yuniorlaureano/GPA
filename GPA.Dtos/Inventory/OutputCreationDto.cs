namespace GPA.Common.DTOs.Inventory
{
    public class OutputCreationDto
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public int TransactionType { get; set; }
        public int Status { get; set; }
        public Guid? StoreId { get; set; }
        public int ReasonId { get; set; }

        public IEnumerable<InputCreationDetailDto> StockDetails { get; set; }

        public StockCreationDto AsStoCreationDto
        {
            get
            {
                return new StockCreationDto
                {
                   Id = this.Id,
                   Description = this.Description,
                   TransactionType = this.TransactionType,
                   Status = this.Status,
                   StoreId = this.StoreId,
                   ReasonId = this.ReasonId,
                   StockDetails = this.StockDetails.Select(x => new StockCreationDetailDto
                   {
                       ProductId = x.ProductId,
                       Quantity = x.Quantity
                   })
                };
            }
        }
    }

    public class InputCreationDetailDto
    {
        public Guid ProductId { get; set;}
        public int Quantity { get; set; }
    }
}
