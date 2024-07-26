using GPA.Entities.General;

namespace GPA.Common.Entities.Inventory
{
    public class StockCycleDetail
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductName { get; set; }
        public ProductType ProductType { get; set; }
        public int Stock { get; set; }
        public int Input { get; set; }
        public int Output { get; set; }
        public CycleType Type { get; set; }
        public Guid StockCycleId { get; set; }

        public StockCycle StockCycle { get; set; }
    }
}
