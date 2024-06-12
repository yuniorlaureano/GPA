using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Inventory
{
    public class StockCycle : Entity<Guid>
    {
        public string Note { get; set; }        
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public CycleType Type { get; set; }

        public Guid? ChildCycleId { get; set; }

        public ICollection<StockCycleDetail> StockCycleDetails { get; set; }
    }
}
