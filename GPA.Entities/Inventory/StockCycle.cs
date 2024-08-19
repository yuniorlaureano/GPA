using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class StockCycle : Entity<Guid>
    {
        public string Note { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsClose { get; set; }

        public ICollection<StockCycleDetail> StockCycleDetails { get; set; }
    }
}
